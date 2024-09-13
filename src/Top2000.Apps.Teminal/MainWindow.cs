using System.Collections.ObjectModel;
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

    private readonly ObservableCollection<TrackListingItem> listings = [];
    private readonly ObservableCollection<TrackListingItem> groups = [];

    public FrameView ListingFrame { get; }

    private int? selectedYear = null;

    public MainWindow(IMediator mediator, TrackInformationView view, HashSet<TrackListing> trackListings, int selectedYear)
    {
        this.mediator = mediator;
        this.trackInformationView = view;
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

        this.ListingFrame.Add(new Top2000ListingListView
        {
            X = 0,
            Y = 0,
            AllowsMultipleSelection = false,
            Height = Dim.Fill(),
            Width = Dim.Fill(),
            OnOpenTrackAsync = this.HandleOpenTrackAsync,
            Source = new Top2000ListingListWrapper(this.listings),
            Top2000Source = new Top2000ListingListWrapper(this.listings),
            Top2000GroupedSource = new Top2000ListingListWrapper(this.groups),
        });

        var infoFrame = new FrameView()
        {
            X = Pos.Right(this.ListingFrame),
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(70) - 10,
            Height = Dim.Fill(),
        };

        infoFrame.Add(view);

        this.Add(menu, this.ListingFrame, infoFrame);
    }


    private async Task HandleOpenTrackAsync(TrackListingItem selectedItem)
    {
        if (selectedItem.TrackId is not null)
        {
            await this.trackInformationView.LoadTrackInformationAsync(selectedItem.TrackId.Value);
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
        var groupsSet = new HashSet<string>();

        this.listings.Clear();
        foreach (var item in trackListings)
        {
            var group = Position(item, this.listings.Count);
            if (groupsSet.Add(group))
            {
                var trackListItem = new TrackListingItem
                {
                    Content = group,
                    ItemType = TrackListingItem.Type.Group,
                };

                this.groups.Add(trackListItem);
                this.listings.Add(trackListItem);
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
