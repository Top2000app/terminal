﻿using MediatR;
using Terminal.Gui;
using Top2000.Apps.Teminal.Views;
using Top2000.Apps.Teminal.Views.ListingView;
using Top2000.Apps.Teminal.Views.SelectEdition;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;

namespace Top2000.Apps.Teminal;

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
        this.trackInformationView = view;
        this.trackListings = trackListings;
        this.ColorScheme = Colors.ColorSchemes["Base"];
        this.SelectedEdition = editions.First();

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

        this.selectEditionDialog = new(editions);

        var menu = new MenuBar
        {
            Menus =
            [
                new MenuBarItem("_File", new MenuItem[] {
                    new("_Selecteer Editie", "", async () => await this.ShowSelectedEditionDialog() ),
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


        this.listingFrame = new()
        {
            X = 0,
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(39),
            Height = Dim.Fill(),
            CanFocus = true,
            Title = editions.First().Year.ToString()
        };



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

        this.listingFrame.Add(this.ListingListView);
        var infoFrame = new FrameView()
        {
            X = Pos.Right(this.listingFrame),
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(70) - 10,
            Height = Dim.Fill(),
        };

        infoFrame.Add(view);
        this.Add(menu, this.listingFrame, infoFrame);
        this.ShowListingsByPosition();

    }

    public Top2000ListingListView ListingListView { get; }
    public Edition SelectedEdition { get; set; }
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

    private async Task ShowSelectedEditionDialog()
    {
        var newEdition = await this.selectEditionDialog.ShowDialogAsync(this.SelectedEdition);

        if (newEdition != this.SelectedEdition)
        {
            this.SelectedEdition = newEdition;
            this.listingFrame.Title = newEdition.Year.ToString();
            await this.LoadEditionAsync();
        }
    }

    private async Task HandleOpenTrackAsync(TrackListingItem selectedItem)
    {
        if (selectedItem.TrackId is not null)
        {
            await this.trackInformationView.LoadTrackInformationAsync(selectedItem.TrackId.Value);
        }
    }

    public async Task LoadEditionAsync()
    {
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

