using Terminal.Gui;

namespace Top2000.Apps.Teminal.Views.ListingView;

public class Top2000ListingListView : ListView
{
    public Top2000ListingListView() : base()
    {
        this.AddCommand(Command.LineDown, () => this.CustomMoveDown());
        this.AddCommand(Command.LineUp, () => this.CustomMoveUp());
        this.OpenSelectedItem += this.ListingOpenSelectedItem;
    }


    private ListViewState State { get; set; }

    private enum ListViewState
    {
        Listing,
        Groups
    }
    public required Top2000ListingListWrapper Top2000GroupedSource
    {
        get;
        set;
    }

    public required Top2000ListingListWrapper Top2000Source
    {
        get;
        set;
    }

    public required Func<TrackListingItem, Task> OnOpenTrackAsync { get; init; }

    private async void ListingOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        var selectedItem = (TrackListingItem)e.Value;

        if (selectedItem.ItemType == TrackListingItem.Type.Group)
        {
            this.HandleOpenGroup(selectedItem);
        }
        else
        {
            await this.OnOpenTrackAsync(selectedItem);
        }
    }

    private void HandleOpenGroup(TrackListingItem selectedItem)
    {
        if (this.State == ListViewState.Groups)
        {
            this.State = ListViewState.Listing;

            this.Source = this.Top2000Source;

            var rows = this.Top2000Source.ToList().IndexOf(selectedItem);

            this.SelectedItem = rows + this.Viewport.Height - 1;
            this.EnsureSelectedItemVisible();
            this.SelectedItem = rows;
        }
        else
        {
            this.State = ListViewState.Groups;
            this.Source = this.Top2000GroupedSource;
        }
    }

    private bool? CustomMoveDown()
    {
        var item = this.Source.ToList()[this.SelectedItem + 1] as TrackListingItem;
        this.MoveDown();

        if (item is not null && item.ItemType != TrackListingItem.Type.Group)
        {
            this.MoveDown();
        }

        return true;
    }

    private bool? CustomMoveUp()
    {
        var item = this.Source.ToList()[this.SelectedItem - 1] as TrackListingItem;
        this.MoveUp();

        if (item is not null && item.ItemType != TrackListingItem.Type.Group)
        {
            this.MoveUp();
        }

        return true;
    }
}