using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Terminal.Gui;

namespace Top2000.Apps.Teminal.Views.ListingView;

public class XX : ListWrapper<TrackListingItem>
{
    public XX(ObservableCollection<TrackListingItem> source) : base(source)
    {
    }


}

public class Top2000ListingListWrapper : IListDataSource, IDisposable
{
    private int _count;
    private BitArray _marks;
    private readonly List<TrackListingItem> _source;

    /// <inheritdoc/>
    public Top2000ListingListWrapper(List<TrackListingItem> source)
    {
        if (source is { })
        {
            this._count = source.Count;
            this._marks = new BitArray(this._count);
            this._source = source;
            this.Length = this.GetMaxLengthItem();
        }
    }

    /// <inheritdoc />
    public int Count => this._source?.Count ?? 0;

    /// <inheritdoc/>
    public int Length { get; private set; }

    private bool _suspendCollectionChangedEvent;
    private bool disposedValue;

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <inheritdoc />
    public bool SuspendCollectionChangedEvent
    {
        get => this._suspendCollectionChangedEvent;
        set
        {
            this._suspendCollectionChangedEvent = value;

            if (!this._suspendCollectionChangedEvent)
            {
                this.CheckAndResizeMarksIfRequired();
            }
        }
    }

    private void CheckAndResizeMarksIfRequired()
    {
        if (this._source != null && this._count != this._source.Count)
        {
            this._count = this._source.Count;
            BitArray newMarks = new BitArray(this._count);
            for (var i = 0; i < Math.Min(this._marks.Length, newMarks.Length); i++)
            {
                newMarks[i] = this._marks[i];
            }
            this._marks = newMarks;

            this.Length = this.GetMaxLengthItem();
        }
    }

    /// <inheritdoc/>
    public void Render(
        ListView container,
        ConsoleDriver driver,
        bool marked,
        int item,
        int col,
        int line,
        int width,
        int start = 0
    )
    {
        var selectedItem = this._source[item];
        bool isSelected = item == container.SelectedItem;

        container.Move(Math.Max(col - start, 0), line);
        //    var t = this._source?[item]?.ToString() ?? "      ";


        if (!isSelected)
        {
            // should be selected? 
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
                // it must be the g, not other selection
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

        this.RenderUstr(driver, selectedItem.Content, col, line, width, start);

    }

    /// <inheritdoc/>
    public bool IsMarked(int item)
    {
        if (item >= 0 && item < this._count)
        {
            return this._marks[item];
        }

        return false;
    }

    /// <inheritdoc/>
    public void SetMark(int item, bool value)
    {
        if (item >= 0 && item < this._count)
        {
            this._marks[item] = value;
        }
    }

    /// <inheritdoc/>
    public IList ToList() { return this._source; }

    /// <inheritdoc/>
    public int StartsWith(string search)
    {
        if (this._source is null || this._source?.Count == 0)
        {
            return -1;
        }

        for (var i = 0; i < this._source.Count; i++)
        {
            object t = this._source[i];

            if (t is string u)
            {
                if (u.ToUpper().StartsWith(search.ToUpperInvariant()))
                {
                    return i;
                }
            }
            else if (t is string s)
            {
                if (s.StartsWith(search, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private int GetMaxLengthItem()
    {
        if (this._source is null || this._source?.Count == 0)
        {
            return 0;
        }

        var maxLength = 0;

        for (var i = 0; i < this._source.Count; i++)
        {
            object t = this._source[i];
            int l;

            if (t is string u)
            {
                l = u.GetColumns();
            }
            else if (t is string s)
            {
                l = s.Length;
            }
            else
            {
                l = t.ToString().Length;
            }

            if (l > maxLength)
            {
                maxLength = l;
            }
        }

        return maxLength;
    }

    private void RenderUstr(ConsoleDriver driver, string ustr, int col, int line, int width, int start = 0)
    {
        string str = start > ustr.GetColumns() ? string.Empty : ustr.Substring(Math.Min(start, ustr.ToRunes().Length - 1));
        string u = TextFormatter.ClipAndJustify(str, width, Alignment.Start);
        driver.AddStr(u);
        width -= u.GetColumns();

        while (width-- > 0)
        {
            driver.AddRune((Rune)' ');
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            this.disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ListingListWrapper()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
