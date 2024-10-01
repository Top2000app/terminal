using Top2000.Apps.Teminal.Theme;

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


        RenderUstr(driver.CurrentAttribute, driver, itemToRender.Content, width, start, ' ', shouldSelectItem);
    }

    public void SetMark(int item, bool value)
    {
        throw new NotSupportedException("Marking items are not supported");
    }

    public IList ToList() => source.ToList();

    public IEnumerable<ListingItem> Groups => source.Where(x => x is ListingItemGroup);

    private static void RenderUstr(Terminal.Gui.Attribute drawingAttribute, ConsoleDriver driver, string ustr, int width, int start, char filler, bool isSelected)
    {
        if (!isSelected)
        {
            if (ustr.StartsWith(Symbols.Up))
            {
                driver.SetAttribute(new(new Color(112, 173, 71), drawingAttribute.Background));
            }

            if (ustr.StartsWith(Symbols.New) || ustr.StartsWith(Symbols.BackInList))
            {
                driver.SetAttribute(new(new Color(255, 192, 0), drawingAttribute.Background));
            }

            if (ustr.StartsWith(Symbols.Down))
            {
                driver.SetAttribute(new(new Color(218, 22, 28), drawingAttribute.Background));
            }

            if (ustr.StartsWith(Symbols.Same))
            {
                driver.SetAttribute(new(Color.Gray, drawingAttribute.Background));
            }
        }

        driver.AddStr(ustr.Substring(0, 2));
        driver.SetAttribute(drawingAttribute);
        ustr = ustr.Substring(2);

        var str = start > ustr.GetColumns() + 2
            ? string.Empty
            : ustr.Substring(Math.Min(start, ustr.ToRunes().Length - 1));

        var u = TextFormatter.ClipAndJustify(str, width - 1, Alignment.Start);
        driver.AddStr(u);
        width -= u.GetColumns() + 2;

        while (width-- > 0)
        {
            driver.AddRune((Rune)filler);
        }
    }
}