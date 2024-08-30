using System.Collections;
using NStack;
using Terminal.Gui;
using Terminal.Gui.Custom;

public class Top2000ListViewDatasource : IMultiLineListDataSource
{
    IList src;
    BitArray marks;
    int count, len;

    /// <inheritdoc/>
    public Top2000ListViewDatasource(IList source)
    {
        if (source != null)
        {
            count = source.Count;
            marks = new BitArray(count);
            src = source;
            len = GetMaxLengthItem();
        }
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            CheckAndResizeMarksIfRequired();
            return src?.Count ?? 0;
        }
    }

    /// <inheritdoc/>
    public int Length => len;

    void CheckAndResizeMarksIfRequired()
    {
        if (src != null && count != src.Count)
        {
            count = src.Count;
            BitArray newMarks = new BitArray(count);
            for (var i = 0; i < Math.Min(marks.Length, newMarks.Length); i++)
            {
                newMarks[i] = marks[i];
            }
            marks = newMarks;

            len = GetMaxLengthItem();
        }
    }

    int GetMaxLengthItem()
    {
        if (src == null || src?.Count == 0)
        {
            return 0;
        }

        int maxLength = 0;
        for (int i = 0; i < src.Count; i++)
        {
            var t = src[i];
            int l;
            if (t is ustring u)
            {
                l = TextFormatter.GetTextWidth(u);
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

    void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
    {
        ustring str = start > ustr.ConsoleWidth ? string.Empty : ustr.Substring(Math.Min(start, ustr.ToRunes().Length - 1));
        ustring u = TextFormatter.ClipAndJustify(str, width, TextAlignment.Left);
        driver.AddStr(u);
        width -= TextFormatter.GetTextWidth(u);
        while (width-- + start > 0)
        {
            driver.AddRune(' ');
        }
    }

    /// <inheritdoc/>
    public void Render(MultiLineListView container, ConsoleDriver driver, bool marked, int item, int col, int line, int width, int start = 0)
    {
        var savedClip = container.ClipToBounds();
        container.Move(Math.Max(col - start, 0), line);
        var t = src?[item]?.ToString() ?? "";

        bool isSelected = item == container.SelectedItem;

        if (!isSelected)
        {
            // should be selected? 
            var otherItem = item;

            if (t.ToString().StartsWith('a'))
            {
                otherItem--; // look at the previous
                isSelected = otherItem == container.SelectedItem;
            }
            else if (t.ToString().StartsWith('t'))
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


        if (t is null)
        {
            RenderUstr(driver, ustring.Make(""), col, line, width);
        }
        else
        {
            this.RenderUstr(driver, t.ToString()[6..], col, line, width, start);
        }
        driver.Clip = savedClip;
    }

    /// <inheritdoc/>
    public bool IsMarked(int item)
    {
        if (item >= 0 && item < Count)
            return marks[item];
        return false;
    }

    /// <inheritdoc/>
    public void SetMark(int item, bool value)
    {
        if (item >= 0 && item < Count)
            marks[item] = value;
    }

    /// <inheritdoc/>
    public IList ToList()
    {
        return src;
    }

    /// <inheritdoc/>
    public int StartsWith(string search)
    {
        if (src == null || src?.Count == 0)
        {
            return -1;
        }

        for (int i = 0; i < src.Count; i++)
        {
            var t = src[i];
            if (t is ustring u)
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
}