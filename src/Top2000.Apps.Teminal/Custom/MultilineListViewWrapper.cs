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
    protected ListingItem(string content)
    {
        Content = content;
    }

    public ListingItem(int id, string content)
    {
        Id = id;
        Content = content;
    }

    public int? Id { get; protected set; }
    public string Content { get; protected set; }
}

public class ListingItemGroup : ListingItem
{
    public ListingItemGroup(string content) : base(content)
    {
        Id = null;
    }
}

public class MultilineListViewWrapper : IListDataSource
{
    private readonly List<ListingItem> source;

    public MultilineListViewWrapper(IEnumerable<ListingItem> source)
    {
        IsGrouped = true;
        this.source = source.ToList();
        Length = GetMaxLengthItem();

        CollectionChanged += (_, __) => { };
    }

    public MultilineListViewWrapper(IEnumerable<MultilineListItemGrouping> groupedSource)
    {
        IsGrouped = true;
        source = [];

        foreach (var group in groupedSource)
        {
            source.Add(group.Key);
            source.AddRange(group);
        }

        Length = GetMaxLengthItem();

        CollectionChanged += (_, __) => { };
    }

    public bool ShowGroupHeader { get; set; }

    public bool IsGrouped
    {
        get;
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

    public Terminal.Gui.Color? GroupColour { get; set; }

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
            if (IsGrouped && itemToRender is ListingItemGroup)
            {
                driver.SetAttribute(new(GroupColour ?? container.ColorScheme.Normal.Foreground, container.ColorScheme.Normal.Background));
            }
            else
            {
                driver.SetAttribute(container.ColorScheme.Normal);
            }
        }

        if (ShowGroupHeader && itemToRender is ListingItemGroup)
        {
            var content = $"\u2528{itemToRender.Content}\u2523";
            RenderUstr(driver, content, width, start, '\u2500');
        }
        else
        {
            RenderUstr(driver, itemToRender.Content, width, start, ' ');
        }

    }

    public void SetMark(int item, bool value)
    {
        throw new NotSupportedException("Marking items are not supported");
    }

    public IList ToList() => source.ToList();

    public IEnumerable<ListingItem> Groups
    {
        get
        {
            return source.Where(x => x is ListingItemGroup);
        }
    }

    private static void RenderUstr(ConsoleDriver driver, string ustr, int width, int start, char filler)
    {
        var str = start > ustr.GetColumns()
            ? string.Empty
            : ustr.Substring(Math.Min(start, ustr.ToRunes().Length - 1));

        var u = TextFormatter.ClipAndJustify(str, width, Alignment.Start);
        driver.AddStr(u);
        width -= u.GetColumns();

        while (width-- > 0)
        {
            driver.AddRune((Rune)filler);
        }
    }
}