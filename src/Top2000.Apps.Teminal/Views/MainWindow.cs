using Top2000.Apps.Teminal.Custom;
using Top2000.Apps.Teminal.Views.SelectEdition;
using Top2000.Apps.Teminal.Views.TrackInformation;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;

namespace Top2000.Apps.Teminal.Views;

public class MainWindow : Toplevel
{
    private readonly IMediator mediator;
    private readonly TrackInformationView trackInformationView;
    private HashSet<TrackListing> trackListings;
    private readonly MenuItem showByPosition;
    private readonly MenuItem showByDate;
    private readonly FrameView listingFrame;
    private readonly SelectEditionDialog selectEditionDialog;
    public MainWindow(IMediator mediator, TrackInformationView view, HashSet<TrackListing> trackListings, SortedSet<Edition> editions)
    {
        this.mediator = mediator;
        trackInformationView = view;
        this.trackListings = trackListings;
        ColorScheme = Colors.ColorSchemes["Base"];
        SelectedEdition = editions.First();

        showByPosition = new("_Show by position", "", ShowListingsByPosition, canExecute: () => SelectedEdition.HasPlayDateAndTime)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = true,
        };

        showByDate = new("_Show by date", "", ShowByDate, canExecute: () => SelectedEdition.HasPlayDateAndTime)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = false,
        };

        selectEditionDialog = new(editions);

        var menu = new MenuBar
        {
            Menus =
            [
                new MenuBarItem("_File", new MenuItem[] {
                    new("_Selecteer Editie", "", async () => await ShowSelectedEditionDialog() ),
                    null,
                    new("_Quit", "", () => { Application.RequestStop (); })
                }),
                new MenuBarItem("_View", new MenuItem[] {
                    showByPosition,
                    showByDate
                }),
                new MenuBarItem("_Help", new MenuItem[] {
                    new("_About", "", () => {})
                }),
            ]
        };

        listingFrame = new()
        {
            X = 0,
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(39),
            Height = Dim.Fill(),
            CanFocus = true,
            Title = editions.First().Year.ToString()
        };

        ListingListView = new()
        {
            X = 0,
            Y = 0,
            AllowsMultipleSelection = false,
            Height = Dim.Fill(),
            Width = Dim.Fill(),
            OnOpenTrackAsync = HandleOpenTrackAsync,
        };

        listingFrame.Add(ListingListView);
        var infoFrame = new FrameView()
        {
            X = Pos.Right(listingFrame),
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(70) - 10,
            Height = Dim.Fill(),
        };

        infoFrame.Add(view);
        Add(menu, listingFrame, infoFrame);
        ShowListingsByPosition();

    }

    public MultilineListView ListingListView { get; }
    public Edition SelectedEdition { get; set; }

    private async Task ShowSelectedEditionDialog()
    {
        var newEdition = await selectEditionDialog.ShowDialogAsync(SelectedEdition);

        if (newEdition != SelectedEdition)
        {
            SelectedEdition = newEdition;
            listingFrame.Title = newEdition.Year.ToString();
            await LoadEditionAsync();
        }
    }

    private async Task HandleOpenTrackAsync(ListingItem selectedItem)
    {
        if (selectedItem.Id is not null)
        {
            await trackInformationView.LoadTrackInformationAsync(selectedItem.Id.Value);
        }
    }

    public async Task LoadEditionAsync()
    {
        trackListings = await mediator.Send(new AllListingsOfEditionRequest { Year = SelectedEdition.Year });

        if (showByDate.Checked.GetValueOrDefault(false) && SelectedEdition.HasPlayDateAndTime)
        {
            ShowByDate();
        }

        if (!SelectedEdition.HasPlayDateAndTime || showByPosition.Checked.GetValueOrDefault(true))
        {
            ShowListingsByPosition();
        }
    }

    private void ShowByDate()
    {
        showByPosition.Checked = false;
        showByDate.Checked = true;

        var list = trackListings.Reverse()
            .GroupByPlayUtcDateAndTime()
            .Select(group =>
                new MultilineListItemGrouping(
                    new ListingItemGroup(PositionDateTime(group.Key)),
                    group.SelectMany(track => new List<ListingItem> {
                        new(track.TrackId, $"{track.Position,-6}{track.Title}"),
                        new(track.TrackId, $"      {track.Artist}")
                        })
                    )
                )
            .ToList();

        ListingListView.Source = new MultilineListViewWrapper(list)
        {
            GroupColour = Terminal.Gui.Color.BrightRed,
            ShowGroupHeader = true,
        };
    }

    public void ShowListingsByPosition()
    {
        showByPosition.Checked = true;
        showByDate.Checked = false;

        var list = trackListings
            .GroupByPosition()
            .Select(group =>
                new MultilineListItemGrouping(
                    new ListingItemGroup(group.Key),
                    group.SelectMany(track => new List<ListingItem> {
                                new(track.TrackId, $"{track.Position,-6}{track.Title}"),
                                new(track.TrackId, $"      {track.Artist}")
                        })
                    )
                )
            .ToList();

        ListingListView.Source = new MultilineListViewWrapper(list)
        {
            GroupColour = Terminal.Gui.Color.BrightRed,
            ShowGroupHeader = true,
        };
    }

    static string PositionDateTime(DateTime utcPlayTime)
    {
        var localTime = utcPlayTime.ToLocalTime();

        var hour = localTime.Hour + 1;
        var date = localTime.ToString("dddd dd MMM H");

        return $"{date}:00 - {hour}:00";
    }
}

