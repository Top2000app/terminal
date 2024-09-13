using System.Collections.ObjectModel;
using System.Data;
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
    private readonly TrackInformationView view;

    private readonly ObservableCollection<TrackListingItem> listings = [];

    public FrameView ListingFrame { get; }

    private readonly ListView ListingListView;
    private int? selectedYear = null;

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

        this.ShowListings(trackListings);

        this.ListingFrame = new()
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
        this.ListingListView.OpenSelectedItem += this.ListingOpenSelectedItem;

        this.ListingFrame.Add(this.ListingListView);

        var infoFrame = new FrameView()
        {
            X = Pos.Right(this.ListingFrame),
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(71),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.None,
        };

        infoFrame.Add(view);

        var top2000DataSource = new Top2000ListingListWrapper(this.listings.ToList());
        this.ListingListView.Source = top2000DataSource;

        this.Add(menu, this.ListingFrame, infoFrame);
    }

    private async void ListingOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        var selectedItem = (TrackListingItem)e.Value;

        if (selectedItem.ItemType == TrackListingItem.Type.Group)
        {
            this.HandleOpenGroup(selectedItem);
        }
        else
        {
            await this.HandleOpenTrackAsync(selectedItem);
        }
    }

    private async Task HandleOpenTrackAsync(TrackListingItem selectedItem)
    {
        if (selectedItem.TrackId is not null)
        {
            await this.view.LoadTrackInformationAsync(selectedItem.TrackId.Value);
        }
    }

    private ListViewState state = ListViewState.Listing;

    private enum ListViewState
    {
        Listing,
        Groups
    }

    private void HandleOpenGroup(TrackListingItem selectedItem)
    {
        if (this.state == ListViewState.Groups)
        {
            this.state = ListViewState.Listing;

            this.ListingListView.Source = new Top2000ListingListWrapper(this.listings.ToList());

            var rows = this.listings.IndexOf(selectedItem);

            this.ListingListView.SelectedItem = rows + this.ListingListView.Viewport.Height - 1;
            this.ListingListView.EnsureSelectedItemVisible();
            this.ListingListView.SelectedItem = rows;

        }
        else
        {
            this.state = ListViewState.Groups;

            var groups = this.listings
             .Where(x => x.ItemType == TrackListingItem.Type.Group)
             .ToList();

            this.ListingListView.Source = new Top2000ListingListWrapper(groups);
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
                this.listings.Add(new TrackListingItem
                {
                    Content = group,
                    ItemType = TrackListingItem.Type.Group,
                });
            }

            this.listings.Add(new TrackListingItem
            {
                ItemType = TrackListingItem.Type.Track,
                Content = $"{item.Position,-6}{item.Title}",
                TrackId = item.TrackId,
            });

            this.listings.Add(new TrackListingItem
            {
                ItemType = TrackListingItem.Type.Artist,
                Content = $"      {item.Artist}",
                TrackId = item.TrackId,
            });
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
