using Top2000.Apps.Teminal.Views;

namespace Top2000.Apps.Teminal.Custom;

public sealed class MultilineListItemGrouping : IGrouping<ListingItemGroup, ListingItem>
{
    private readonly IEnumerable<ListingItem> items;

    public MultilineListItemGrouping(ListingItemGroup key, IEnumerable<ListingItem> items)
    {
        Key = key;
        this.items = items;
    }

    public ListingItemGroup Key { get; }

    public IEnumerator<ListingItem> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
}

public class ListingItem
{
    public ListingItem(int id, string content)
    {
        Id = id;
        Content = content;
        SearchContent = content;
    }

    public ListingItem(int id, string content, string searchContent)
    {
        Id = id;
        Content = content;
        SearchContent = searchContent;
    }

    public int? Id { get; protected set; }
    public string Content { get; protected set; }

    public string SearchContent { get; protected set; }

    public Color? ForeColour { get; set; }
}

public class ListingItemGroup : ListingItem
{
    public ListingItemGroup(string content) : base(0, content)
    {
        Id = null;
    }
}

public class DoubleGroupedListViewWrapper : MultilineListViewWrapper
{
    public DoubleGroupedListViewWrapper(IEnumerable<MultilineListItemGrouping> groupedSource, IEnumerable<MultilineListItemGrouping> groupedGroups)
        : base(groupedSource)
    {
        this.GroupedGroups = groupedGroups;

        GroupedMultiLineViewWrapper = new MultilineListViewWrapper(groupedGroups);
    }

    public IEnumerable<MultilineListItemGrouping> GroupedGroups { get; }
}

public class MultilineListViewWrapper : IListDataSource
{
    protected readonly List<ListingItem> source;

    public MultilineListViewWrapper(IEnumerable<ListingItem> source)
    {
        IsGrouped = true;
        this.source = source.ToList();
        Length = GetMaxLengthItem();

        CollectionChanged += (_, __) => { };
        GroupedMultiLineViewWrapper = this;
    }

    public MultilineListViewWrapper(IEnumerable<MultilineListItemGrouping> groupedSource)
    {
        IsGrouped = true;
        source = [];

        var groupId = 0;
        var groups = new List<ListingItem>();

        foreach (var group in groupedSource)
        {
            groups.Add(new ListingItem(groupId++, group.Key.Content));

            source.Add(group.Key);
            source.AddRange(group);
        }

        Length = GetMaxLengthItem();

        CollectionChanged += (_, __) => { };

        GroupedMultiLineViewWrapper = new MultilineListViewWrapper(groups);
    }

    public bool IsGrouped
    {
        get;
    }

    public MultilineListViewWrapper GroupedMultiLineViewWrapper
    {
        get; protected set;
    }

    public ListingItem? this[int index]
    {
        get
        {
            if (index < 0 || index > source.Count - 1)
            {
                return null;
            }

            return source[index];
        }
    }

    private int GetMaxLengthItem()
    {
        var maxLength = 0;

        foreach (var item in source)
        {
            var l = item.Content.Length;
            if (l > maxLength)
            {
                maxLength = l;
            }
        }

        return maxLength;
    }

    public int Count => source.Count;

    public int Length { get; }

    public bool SuspendCollectionChangedEvent { get; set; }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public bool IsMarked(int item)
    {
        throw new NotSupportedException("Marking is not supported");
    }

    public List<int> IndexesOf(int id)
    {
        var indexes = new List<int>();

        for (var i = 0; i < source.Count; i++)
        {
            if (source[i].Id == id)
            {
                indexes.Add(i);
            }
        }

        return indexes;
    }

    public int FindIndexOf(string content)
    {
        for (var i = 0; i < source.Count; i++)
        {
            if (source[i].Content == content)
            {
                return i;
            }
        }

        return -1;
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        var itemToRender = source[item];
        var shouldSelectItem = item == container.SelectedItem;

        container.Move(Math.Max(col - start, 0), line);

        if (!shouldSelectItem)
        {
            var selectedItem = this[container.SelectedItem];
            if (selectedItem is not null && selectedItem.Id.HasValue)
            {
                // all item with the same Id should be visible selected
                shouldSelectItem = selectedItem.Id == itemToRender.Id;
            }
        }

        if (shouldSelectItem)
        {
            driver.SetAttribute(container.ColorScheme.Focus);
        }
        else
        {
            driver.SetAttribute(new(itemToRender.ForeColour ?? container.ColorScheme.Normal.Foreground, container.ColorScheme.Normal.Background));
        }


        RenderUstr(container, driver, itemToRender.Content, width, start, ' ');
    }

    public void SetMark(int item, bool value)
    {
        throw new NotSupportedException("Marking items are not supported");
    }

    public IList ToList() => source.ToList();

    public IEnumerable<ListingItem> Groups => source.Where(x => x is ListingItemGroup);

    private static void RenderUstr(ListView container, ConsoleDriver driver, string ustr, int width, int start, char filler)
    {
        var green = ustr.IndexOf(Symbols.Up, StringComparison.CurrentCultureIgnoreCase);
        var yellow = ustr.IndexOf(Symbols.New, StringComparison.CurrentCultureIgnoreCase);
        var alsoYellow = ustr.IndexOf(Symbols.BackInList, StringComparison.CurrentCultureIgnoreCase);
        var red = ustr.IndexOf("\uFC2C", StringComparison.CurrentCultureIgnoreCase);

        var skip = 0;

        if (green != -1 || yellow != -1 || red != -1 || alsoYellow != -1)
        {
            if (green != -1)
            {
                driver.SetAttribute(new(new Color(112, 173, 71), container.ColorScheme.Normal.Background));
                driver.AddStr(Symbols.New);
                skip = 1;
            }

            if (yellow != -1)
            {
                driver.SetAttribute(new(new Color(255, 192, 0), container.ColorScheme.Normal.Background));
                driver.AddStr(Symbols.New);
                skip = 2;
            }

            if (alsoYellow != -1)
            {
                driver.SetAttribute(new(new Color(255, 192, 0), container.ColorScheme.Normal.Background));
                driver.AddStr(Symbols.BackInList);
                skip = 1;
            }

            if (red != -1)
            {
                driver.SetAttribute(new(new Color(218, 22, 28), container.ColorScheme.Normal.Background));
                driver.AddStr("\uFC2C");
                skip = 1;
            }

            ustr = ustr.Substring(skip);
            driver.SetAttribute(new(container.ColorScheme.Normal.Foreground, container.ColorScheme.Normal.Background));
        }

        var str = start > ustr.GetColumns() + skip
            ? string.Empty
            : ustr.Substring(Math.Min(start, ustr.ToRunes().Length - 1));

        var u = TextFormatter.ClipAndJustify(str, width - 1, Alignment.Start);
        driver.AddStr(u);
        width -= u.GetColumns() + skip;

        while (width-- > 0)
        {
            driver.AddRune((Rune)filler);
        }
    }
}