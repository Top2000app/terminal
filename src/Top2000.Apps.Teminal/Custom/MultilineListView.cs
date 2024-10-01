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
            GroupedSource = value.GroupedMultiLineViewWrapper;
        }
    }

    public bool IsGroupedByDate { get; set; }

    public required Func<ListingItem, Task> OnOpenTrackAsync { get; init; }

    private async void ListingOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        if (State == ListViewState.Groups || e.Value is ListingItemGroup)
        {
            HandleOpenGroup((ListingItem)e.Value);
        }
        else
        {
            await OnOpenTrackAsync((ListingItem)e.Value);
        }
    }

    private void HandleOpenGroup(ListingItem selectedItem)
    {
        if (State == ListViewState.Groups)
        {
            var rows = OriginalSource.FindIndexOf(selectedItem.SearchContent);
            if (rows == -1)
            {
                return;
            }

            State = ListViewState.Listing;

            base.Source = OriginalSource;
            var x = rows + Viewport.Height - 1;

            SelectedItem = Math.Min(x, Source.Count - 1);
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