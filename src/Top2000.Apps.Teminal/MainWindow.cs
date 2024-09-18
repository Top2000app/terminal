using MediatR;
using Terminal.Gui;
using Top2000.Apps.Teminal.Views;
using Top2000.Apps.Teminal.Views.ListingView;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;

namespace Top2000.Apps.Teminal;

public class MainWindow : Toplevel
{
    private readonly IMediator mediator;
    private readonly TrackInformationView trackInformationView;
    private HashSet<TrackListing> trackListings;
    private readonly SortedSet<Edition> editions;

    public FrameView ListingFrame { get; }
    public Top2000ListingListView ListingListView { get; }

    private TableView selectedEditionList;
    private EditionsDataSource editionDataSource;
    private Edition selectedEdition;
    private readonly MenuItem showByPosition;
    private readonly MenuItem showByDate;

    public MainWindow(IMediator mediator, TrackInformationView view, HashSet<TrackListing> trackListings, SortedSet<Edition> editions)
    {
        this.mediator = mediator;
        this.trackInformationView = view;
        this.trackListings = trackListings;
        this.editions = editions;
        this.ColorScheme = Colors.ColorSchemes["Base"];

        this.showByPosition = new("_Show by position", "", () => this.ShowListingsByPosition(), canExecute: () => this.SelectedEdition.HasPlayDateAndTime)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = true,
        };

        this.showByDate = new("_Show by date", "", () => this.ShowByDate(), canExecute: () => this.SelectedEdition.HasPlayDateAndTime)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = false,
        };

        var menu = new MenuBar
        {
            Menus =
            [
                new MenuBarItem("_File", new MenuItem[] {
                    new("_Selecteer Editie", "", this.ShowSelectedEdition ),
                    null,
                    new("_Quit", "", () => { Application.RequestStop (); })
                }),
                new MenuBarItem("_View", new MenuItem[] {
                    this.showByPosition,
                    this.showByDate
                }),
                new MenuBarItem("_Help", new MenuItem[] {
                    new("_About", "", () => {})
                }),
            ]
        };


        this.ListingFrame = new()
        {
            X = 0,
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(39),
            Height = Dim.Fill(),
            CanFocus = true,
            Title = editions.First().Year.ToString()
        };

        this.SelectedEdition = editions.First();


        this.ListingListView = new()
        {
            X = 0,
            Y = 0,
            AllowsMultipleSelection = false,
            Height = Dim.Fill(),
            Width = Dim.Fill(),
            OnOpenTrackAsync = this.HandleOpenTrackAsync,
            Source = new Top2000ListingListWrapper([]),
            Top2000Source = new Top2000ListingListWrapper([]),
            Top2000GroupedSource = new Top2000ListingListWrapper([]),
        };

        this.ListingFrame.Add(this.ListingListView);
        var infoFrame = new FrameView()
        {
            X = Pos.Right(this.ListingFrame),
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(70) - 10,
            Height = Dim.Fill(),
        };

        infoFrame.Add(view);
        this.Add(menu, this.ListingFrame, infoFrame);
        this.ShowListingsByPosition();

    }

    private void ShowByDate()
    {
        this.showByPosition.Checked = false;
        this.showByDate.Checked = true;

        var groupsSet = new HashSet<string>();

        var list = new List<TrackListingItem>();
        foreach (var item in this.trackListings.Reverse())
        {
            var group = PositionDateTime(item);
            if (groupsSet.Add(group))
            {
                var trackListItem = new TrackListingItem
                {
                    Content = group,
                    ItemType = TrackListingItem.Type.Group,
                };

                list.Add(trackListItem);
            }

            list.Add(new TrackListingItem
            {
                ItemType = TrackListingItem.Type.Track,
                Content = $"{item.Position,-6}{item.Title}",
                TrackId = item.TrackId,
            });

            list.Add(new TrackListingItem
            {
                ItemType = TrackListingItem.Type.Artist,
                Content = $"      {item.Artist}",
                TrackId = item.TrackId,
            });
        }

        this.ListingListView.Source = new Top2000ListingListWrapper(list);
        this.ListingListView.Top2000Source = new Top2000ListingListWrapper(list);
        this.ListingListView.Top2000GroupedSource = new Top2000ListingListWrapper(list.Where(x => x.ItemType == TrackListingItem.Type.Group).ToList());
    }

    private void ShowSelectedEdition()
    {
        this.editionDataSource = new EditionsDataSource(this.editions, this.SelectedEdition.Year);

        var okButton = new Button { Text = "_Show" };
        okButton.Accept += (s, e) =>
        {
            Application.RequestStop();
        };

        var cancelButton = new Button { Text = "_Cancel" };
        cancelButton.Accept += (s, e) =>
        {
            Application.RequestStop();
        };

        Dialog dialog = new()
        {
            Title = "Selecteer editie",
            Width = 72,
            Height = this.editionDataSource.Rows * 2,
            Buttons = [okButton, cancelButton],
            ButtonAlignment = Alignment.Center
        };

        this.selectedEditionList = new TableView(this.editionDataSource)
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Height = this.editionDataSource.Rows,
            Width = 36,
            Style = new TableStyle
            {
                ShowHorizontalScrollIndicators = false,
                ShowHorizontalBottomline = false,
                ShowHeaders = false,
                ExpandLastColumn = false,
                ShowHorizontalHeaderOverline = false,
                ShowVerticalHeaderLines = false,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalCellLines = false,
            }
        };

        this.selectedEditionList.SetSelection(this.editionDataSource.SelectedRowColumn.Item2, this.editionDataSource.SelectedRowColumn.Item1, extendExistingSelection: false);

        dialog.Add(this.selectedEditionList);

        dialog.Closed += this.Dialog_Closed;

        Application.Run(dialog);
    }

    private async void Dialog_Closed(object? sender, ToplevelEventArgs e)
    {
        var result = this.editionDataSource[this.selectedEditionList.SelectedRow, this.selectedEditionList.SelectedColumn]?.ToString();

        if (result is not null)
        {
            await this.LoadItAsync(int.Parse(result));
        }
    }

    private async Task HandleOpenTrackAsync(TrackListingItem selectedItem)
    {
        if (selectedItem.TrackId is not null)
        {
            await this.trackInformationView.LoadTrackInformationAsync(selectedItem.TrackId.Value);
        }
    }

    public async Task LoadItAsync(int year)
    {
        this.SelectedEdition = this.editions.First(x => x.Year == year);
        this.trackListings = await this.mediator.Send(new AllListingsOfEditionRequest { Year = this.SelectedEdition.Year });

        if (this.showByDate.Checked.GetValueOrDefault(false) && this.SelectedEdition.HasPlayDateAndTime)
        {
            this.ShowByDate();
        }

        if (!this.SelectedEdition.HasPlayDateAndTime || this.showByPosition.Checked.GetValueOrDefault(true))
        {
            this.ShowListingsByPosition();
        }
    }

    public Edition SelectedEdition
    {
        get { return this.selectedEdition; }
        set
        {
            this.ListingFrame.Title = value.Year.ToString();
            this.selectedEdition = value;
        }
    }

    public void ShowListingsByPosition()
    {
        this.showByPosition.Checked = true;
        this.showByDate.Checked = false;

        var groupsSet = new HashSet<string>();

        var list = new List<TrackListingItem>();
        foreach (var item in this.trackListings)
        {
            var group = PositionGroup(item, this.trackListings.Count);
            if (groupsSet.Add(group))
            {
                var trackListItem = new TrackListingItem
                {
                    Content = group,
                    ItemType = TrackListingItem.Type.Group,
                };

                list.Add(trackListItem);
            }

            list.Add(new TrackListingItem
            {
                ItemType = TrackListingItem.Type.Track,
                Content = $"{item.Position,-6}{item.Title}",
                TrackId = item.TrackId,
            });

            list.Add(new TrackListingItem
            {
                ItemType = TrackListingItem.Type.Artist,
                Content = $"      {item.Artist}",
                TrackId = item.TrackId,
            });
        }

        this.ListingListView.Source = new Top2000ListingListWrapper(list);
        this.ListingListView.Top2000Source = new Top2000ListingListWrapper(list);
        this.ListingListView.Top2000GroupedSource = new Top2000ListingListWrapper(list.Where(x => x.ItemType == TrackListingItem.Type.Group).ToList());
    }

    static string PositionDateTime(TrackListing listing)
    {
        var localTime = listing.PlayUtcDateAndTime.ToLocalTime();

        var hour = localTime.Hour + 1;
        var date = localTime.ToString("dddd dd MMM H");

        return $"{date}:00 - {hour}:00";
    }

    static string PositionGroup(TrackListing listing, int countOfItems)
    {
        const int GroupSize = 100;

        if (listing.Position < 100) return "1 - 100";

        if (countOfItems > 2000)
        {
            if (listing.Position >= 2400) return "2400 - 2500";
        }
        else
        {
            if (listing.Position >= 1900) return "1900 - 2000";
        }

        var min = listing.Position / GroupSize * GroupSize;
        var max = min + GroupSize;

        return $"{min} - {max}";
    }


}

public class EditionsDataSource : ITableSource
{
    private readonly string[][] items;

    public EditionsDataSource(SortedSet<Edition> editions, int selectedYear)
    {
        this.Columns = 5;
        this.Rows = editions.Count / this.Columns;

        this.items = new string[this.Rows][];
        for (var row = 0; row < this.Rows; row++)
        {
            this.items[row] = new string[this.Columns];
        }

        var index = 0;
        foreach (var edition in editions.Reverse())
        {
            var row = this.Rows - (index / this.Columns) - 1;
            var col = index % this.Columns;
            this.items[row][col] = $" {edition.Year} ";

            if (edition.Year == selectedYear)
            {
                this.SelectedRowColumn = new Tuple<int, int>(row, col);
            }

            index++;
        }
    }

    public Tuple<int, int> SelectedRowColumn { get; }

    public object this[int row, int col] => this.items[row][col];

    public string[] ColumnNames => ["", "", "", "", ""];

    public int Columns { get; }

    public int Rows { get; }
}
