using Terminal.Gui;

namespace Top2000.Apps.Teminal.Views.ListingView;

public class Top2000ListingListView : ListView
{
    public Top2000ListingListView() : base()
    {
        this.AddCommand(Command.LineDown, () => this.CustomMoveDown());
        this.AddCommand(Command.LineUp, () => this.CustomMoveUp());
        this.OpenSelectedItem += this.ListingOpenSelectedItem;
        Top2000Source = new Top2000ListingListWrapper(new List<ListingItem>());
        Top2000GroupedSource = new Top2000ListingListWrapper(new List<ListingItem>());
    }

    private ListViewState State { get; set; }

    private enum ListViewState
    {
        Listing,
        Groups
    }

    private Top2000ListingListWrapper Top2000GroupedSource
    {
        get;
        set;
    }

    private Top2000ListingListWrapper Top2000Source
    {
        get;
        set;
    }

    public new virtual Top2000ListingListWrapper Source
    {
        get
        {
            return Top2000Source;
        }
        set
        {
            base.Source = value;
            Top2000Source = value;
            Top2000GroupedSource = new Top2000ListingListWrapper(value.Groups.ToList());
        }
    }

    public required Func<ListingItem, Task> OnOpenTrackAsync { get; init; }

    private async void ListingOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        var selectedItem = (ListingItem)e.Value;

        if (selectedItem is ListingItemGroup group)
        {
            this.HandleOpenGroup(group);
        }
        else
        {
            await this.OnOpenTrackAsync(selectedItem);
        }
    }

    private void HandleOpenGroup(ListingItemGroup selectedItem)
    {
        if (this.State == ListViewState.Groups)
        {
            this.State = ListViewState.Listing;

            base.Source = this.Top2000Source;

            var rows = this.Top2000Source.ToList().IndexOf(selectedItem);

            this.SelectedItem = rows + this.Viewport.Height - 1;
            this.EnsureSelectedItemVisible();
            this.SelectedItem = rows;
        }
        else
        {
            this.State = ListViewState.Groups;
            base.Source = this.Top2000GroupedSource;
        }
    }

    private bool? CustomMoveDown()
    {
        var index = this.SelectedItem + 1;

        if (index > this.Source.Count - 1)
        {
            return false;
        }

        this.MoveDown();

        var item = this.Source[index];
        if (item is not null && item is not ListingItemGroup)
        {
            this.MoveDown();
        }

        return true;
    }

    private bool? CustomMoveUp()
    {
        var index = this.SelectedItem - 1;
        if (index < 0)
        {
            return false;
        }

        this.MoveUp();

        var item = this.Source[this.SelectedItem - 1];
        if (item is not null && item is not ListingItemGroup)
        {
            this.MoveUp();
        }

        return true;
    }
}