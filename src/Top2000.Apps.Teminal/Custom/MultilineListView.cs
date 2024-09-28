using Terminal.Gui;

namespace Top2000.Apps.Teminal.Custom;

public class MultilineListView : ListView
{
    public MultilineListView() : base()
    {
        AddCommand(Command.LineDown, () => MultilineMoveDown());
        AddCommand(Command.LineUp, () => MultilineMoveUp());
        OpenSelectedItem += ListingOpenSelectedItem;
        OriginalSource = new MultilineListViewWrapper(new List<ListingItem>());
        GroupedSource = new MultilineListViewWrapper(new List<ListingItem>());
    }

    private ListViewState State { get; set; }

    private enum ListViewState
    {
        Listing,
        Groups
    }

    private MultilineListViewWrapper GroupedSource
    {
        get;
        set;
    }

    private MultilineListViewWrapper OriginalSource
    {
        get;
        set;
    }

    public new virtual MultilineListViewWrapper Source
    {
        get
        {
            return OriginalSource;
        }
        set
        {
            base.Source = value;
            OriginalSource = value;
            GroupedSource = new MultilineListViewWrapper(value.Groups.ToList());
        }
    }

    public required Func<ListingItem, Task> OnOpenTrackAsync { get; init; }

    private async void ListingOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        var selectedItem = (ListingItem)e.Value;

        if (selectedItem is ListingItemGroup group)
        {
            HandleOpenGroup(group);
        }
        else
        {
            await OnOpenTrackAsync(selectedItem);
        }
    }

    private void HandleOpenGroup(ListingItemGroup selectedItem)
    {
        if (State == ListViewState.Groups)
        {
            State = ListViewState.Listing;

            base.Source = OriginalSource;

            var rows = OriginalSource.ToList().IndexOf(selectedItem);

            SelectedItem = rows + Viewport.Height - 1;
            EnsureSelectedItemVisible();
            SelectedItem = rows;
        }
        else
        {
            State = ListViewState.Groups;
            base.Source = GroupedSource;
        }
    }

    private bool? MultilineMoveDown()
    {
        if (SelectedItem + 1 > Source.Count - 1)
        {
            return false;
        }

        var currentItem = Source[SelectedItem];

        ListingItem? newItem;

        do
        {
            MoveDown();
            newItem = Source[SelectedItem];
        }
        while (newItem is not null && newItem is not ListingItemGroup && newItem.Id == currentItem?.Id);

        return true;
    }

    private bool? MultilineMoveUp()
    {
        if (SelectedItem - 1 < 0)
        {
            return false;
        }

        var currentItem = Source[SelectedItem];

        ListingItem? newItem;

        do
        {
            MoveUp();
            newItem = Source[SelectedItem];
        }
        while (newItem is not null && newItem is not ListingItemGroup && newItem.Id == currentItem?.Id);

        return true;
    }
}