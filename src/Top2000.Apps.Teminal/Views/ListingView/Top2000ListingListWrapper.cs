using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Terminal.Gui;

namespace Top2000.Apps.Teminal.Views.ListingView;

public class Top2000ListingListWrapper : IListDataSource
{
    private readonly ObservableCollection<TrackListingItem> source;

    public Top2000ListingListWrapper(ObservableCollection<TrackListingItem> source)
    {
        this.source = source;
        this.Count = source.Count;
        source.CollectionChanged += this.SourceCollectionChanged;
        this.Length = this.GetMaxLengthItem();
        CollectionChanged += this.SourceCollectionChanged;
    }

    public TrackListingItem? this[int index]
    {
        get
        {
            if (index < 0)
            {
                return null;
            }

            if (index > this.source.Count - 1)
            {
                return null;
            }

            return this.source[index];
        }
    }

    private int GetMaxLengthItem()
    {
        var maxLength = 0;

        foreach (var item in this.source)
        {
            var l = item.Content.Length;
            if (l > maxLength)
            {
                maxLength = l;
            }
        }

        return maxLength;
    }

    public int Count { get; }

    public int Length { get; }

    public bool SuspendCollectionChangedEvent { get; set; }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!this.SuspendCollectionChangedEvent)
        {
            CollectionChanged?.Invoke(sender, e);
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        this.source.CollectionChanged -= this.SourceCollectionChanged;
        CollectionChanged -= this.SourceCollectionChanged;
    }

    public bool IsMarked(int item)
    {
        throw new NotSupportedException("Marking is not supported");
    }

    public void Render(ListView container, ConsoleDriver driver, bool marked, int item, int col, int line, int width, int start = 0)
    {
        var selectedItem = this.source[item];
        var isSelected = item == container.SelectedItem;

        container.Move(Math.Max(col - start, 0), line);

        if (!isSelected)
        {
            var otherItem = item;

            if (selectedItem.ItemType == TrackListingItem.Type.Artist)
            {
                otherItem--; // look at the previous
                isSelected = otherItem == container.SelectedItem;
            }
            else if (selectedItem.ItemType == TrackListingItem.Type.Track)
            {
                otherItem++; // look at the next
                isSelected = otherItem == container.SelectedItem;
            }
            else
            {
                // it must be the group, not other selection
            }
        }

        if (isSelected)
        {
            driver.SetAttribute(container.ColorScheme.Focus);
        }
        else
        {
            driver.SetAttribute(container.ColorScheme.Normal);
        }

        RenderUstr(driver, selectedItem.Content, width, start);
    }

    public void SetMark(int item, bool value)
    {
        throw new NotSupportedException("Marking items are not supported");
    }

    public IList ToList() => this.source.ToList();

    private static void RenderUstr(ConsoleDriver driver, string ustr, int width, int start = 0)
    {
        var str = start > ustr.GetColumns() ? string.Empty : ustr.Substring(Math.Min(start, ustr.ToRunes().Length - 1));
        var u = TextFormatter.ClipAndJustify(str, width, Alignment.Start);
        driver.AddStr(u);
        width -= u.GetColumns();

        while (width-- > 0)
        {
            driver.AddRune((Rune)' ');
        }
    }
}