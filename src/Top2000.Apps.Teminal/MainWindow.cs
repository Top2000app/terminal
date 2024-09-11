using System.Collections.ObjectModel;
using MediatR;
using Terminal.Gui;
using Top2000.Apps.Teminal.Views;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;

namespace Top2000.Apps.Teminal;

public class MainWindow : Toplevel
{
    private readonly IMediator mediator;
    private readonly TrackInformationView view;

    private readonly ObservableCollection<string> listings = [];

    private readonly ListView ListingListView;
    private int? selectedYear;

    public MainWindow(IMediator mediator, TrackInformationView view, HashSet<TrackListing> trackListings, int selectedYear)
    {
        this.mediator = mediator;
        this.view = view;
        this.ColorScheme = Colors.ColorSchemes["Base"];

        var menu = new MenuBar
        {
            Menus =
            [
                new MenuBarItem("_File", new MenuItem[] {
                    new("_Quit", "", () => {
                        Application.RequestStop ();
                    })
                }),
                new MenuBarItem("_Help", new MenuItem[] {
                    new("_About", "", async () => {
                        await this.LoadItAsync();
                    })
                }),
            ]
        };

        //var info = new View
        //{
        //    X = Pos.Percent(36),
        //    Y = 0,
        //    Width = Dim.Fill(),
        //    Height = Dim.Fill(),
        //};

        this.ShowListings(trackListings);



        var listFrame = new FrameView()
        {
            X = 0,
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(39),
            Height = Dim.Fill(),
            CanFocus = true,
            Title = selectedYear.ToString()
        };

        this.ListingListView = new()
        {
            X = 0,
            Y = 0,
            AllowsMultipleSelection = false,
            Height = Dim.Fill(),
            Width = Dim.Fill(),
        };
        this.ListingListView.SelectedItemChanged += this.ListingListView_SelectedItemChanged;

        listFrame.Add(this.ListingListView);

        var infoFrame = new FrameView()
        {
            X = Pos.Right(listFrame),
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(71),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.None,
        };

        infoFrame.Add(view);

        var top2000DataSource = new ListingListWrapper<string>(this.listings.ToList());
        this.ListingListView.Source = top2000DataSource;

        this.Add(menu, listFrame, infoFrame);
    }


    private bool IsListingVisible = true;
    private async void ListingListView_SelectedItemChanged(object? sender, ListViewItemEventArgs e)
    {
        if (this.IsListingVisible)
        {
            if (e.Value is string selectedItem)
            {
                if (selectedItem[0] == 'g')
                {
                    // group select, change the list
                    this.IsListingVisible = false;

                    var groups = this.listings
                        .Where(x => x[0] == 'g')
                        .ToList();

                    var top2000DataSource = new ListingListWrapper<string>(groups);
                    this.ListingListView.Source = top2000DataSource;
                }

                if (selectedItem[0] == 'a' || selectedItem[0] == 't')
                {
                    if (int.TryParse(selectedItem[1..5].Trim(), out var trackId))
                    {
                        await this.view.LoadTrackInformationAsync(trackId);
                    }
                }
            }
        }
        else
        {
            if (e.Value is string selectedGroup)
            {
                // groups are shown
                this.IsListingVisible = true;
                var top2000DataSource = new ListingListWrapper<string>(this.listings.ToList());
                this.ListingListView.Source = top2000DataSource;

                var rows = this.listings.IndexOf(selectedGroup);
                var didit = this.ListingListView.ScrollVertical(rows + 1);
                this.SetNeedsDisplay();

            }
        }
    }

    public async Task LoadItAsync()
    {
        var editions = await this.mediator.Send(new AllEditionsRequest()).ConfigureAwait(false);
        this.selectedYear = editions.First().Year;
        var listingsResults = await this.mediator.Send(new AllListingsOfEditionRequest { Year = this.selectedYear.Value });

        this.ShowListings(listingsResults);
    }

    public void ShowListings(HashSet<TrackListing> trackListings)
    {
        var groups = new HashSet<string>();

        this.listings.Clear();
        foreach (var item in trackListings)
        {
            var group = Position(item, this.listings.Count);
            if (groups.Add(group))
            {
                this.listings.Add("g     " + group);
            }

            this.listings.Add("t" + item.TrackId.ToString().PadRight(5, ' ') + item.Position.ToString().PadRight(6, ' ') + item.Title);
            this.listings.Add("a" + item.TrackId.ToString().PadRight(5, ' ') + "      " + item.Artist);
        }
    }

    static string Position(TrackListing listing, int countOfItems)
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
