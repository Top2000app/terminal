using System.Collections;
using Figgle;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NStack;
using Terminal.Gui;
using Top2000.Data.ClientDatabase;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;
using Top2000.Features.SQLite;

Console.WriteLine(
    FiggleFonts.Standard.Render("TOP2000 Terminal!"));

var services = new ServiceCollection()
    .AddClientDatabase(new DirectoryInfo(Directory.GetCurrentDirectory()))
    .AddFeaturesWithSQLite()
    ;

var serviceProvider = services.BuildServiceProvider();

var assemblySource = serviceProvider.GetRequiredService<Top2000AssemblyDataSource>();
var update = serviceProvider.GetRequiredService<IUpdateClientDatabase>();

Console.WriteLine("Settings up Top2000 database");

await update.RunAsync(assemblySource);

var onlineSource = serviceProvider.GetRequiredService<OnlineDataSource>();
var updateOnline = serviceProvider.GetRequiredService<IUpdateClientDatabase>();

Console.WriteLine("Updating Top2000 database");

await updateOnline.RunAsync(onlineSource);

var mediator = serviceProvider.GetRequiredService<IMediator>();

var editions = await mediator.Send(new AllEditionsRequest());
var listings = await mediator.Send(new AllListingsOfEditionRequest { Year = 2022 });

var toShow = new List<string>();

foreach (var item in listings)
{
    toShow.Add(item.Position.ToString().PadRight(6, ' ') + item.Title);
    toShow.Add("      " + item.Artist);
}

var try1 = new CustomListWrapper(toShow);

Application.Init();
var menu = new MenuBar([
            new MenuBarItem ("_Bestand", new MenuItem [] {
                new("_Sluiten", "", () => {
                    Application.RequestStop ();
                })
            }),
            new MenuBarItem ("_Edities", editions.Select(x =>
                new MenuItem (x.Year.ToString(), "", () => {
                    Application.RequestStop();
                })).ToArray()
            )
        ]);

var win = new Window(editions.First().Year.ToString())
{
    X = 0,
    Y = 1,
    Width = Dim.Fill(),
    Height = Dim.Fill() - 1
};

var list = new ListView()
{
    X = 1,
    Y = 1,
    AllowsMultipleSelection = false,
    Height = Dim.Fill(),
    Width = Dim.Fill(1),
};

win.Add(list);

////list.SetSource(listings.ToList());
////

list.Source = try1;

//var scrollBar = new ScrollBarView(list, true);
//scrollBar.ChangedPosition += () =>
//{
//    list.TopItem = scrollBar.Position;

//    if (list.TopItem != scrollBar.Position)
//    {
//        scrollBar.Position = list.TopItem;
//    }

//    list.SetNeedsDisplay();
//};

//scrollBar.OtherScrollBarView.ChangedPosition += () =>
//{
//    list.LeftItem = scrollBar.OtherScrollBarView.Position;

//    if (list.LeftItem != scrollBar.OtherScrollBarView.Position)
//    {
//        scrollBar.OtherScrollBarView.Position = list.LeftItem;
//    }

//    list.SetNeedsDisplay();
//};

//list.DrawContent += (obj) =>
//{
//    scrollBar.Size = list.Source.Count;
//    scrollBar.Position = list.TopItem;
//    scrollBar.OtherScrollBarView.Size = list.Source.Length;
//    scrollBar.OtherScrollBarView.Position = list.LeftItem;
//    scrollBar.Refresh();
//};

// Add both menu and win in a single call
Application.Top.Add(menu, win);
Application.Run();
Application.Shutdown();


public class CustomListWrapper : IListDataSource
{
    IList src;
    BitArray marks;
    int count, len;

    /// <inheritdoc/>
    public CustomListWrapper(IList source)
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
    public void Render(ListView container, ConsoleDriver driver, bool marked, int item, int col, int line, int width, int start = 0)
    {
        var savedClip = container.ClipToBounds();
        container.Move(Math.Max(col - start, 0), line);
        var t = src?[item];

        bool isSelected = item == container.SelectedItem;

        if (!isSelected)
        {
            // should be selected? 
            var otherItem = item;

            if (t.ToString().StartsWith("  "))
            {
                otherItem--; // look at the previous
            }
            else
            {
                otherItem++; // look at the next
            }

            isSelected = otherItem == container.SelectedItem;
        }

        if (isSelected)
        {
            driver.SetAttribute(container.ColorScheme.Focus);
        }
        else
        {
            driver.SetAttribute(container.ColorScheme.Normal);
        }


        if (t == null)
        {
            RenderUstr(driver, ustring.Make(""), col, line, width);
        }
        else
        {
            if (t is ustring u)
            {
                RenderUstr(driver, u, col, line, width, start);
            }
            else if (t is string s)
            {
                RenderUstr(driver, s, col, line, width, start);
            }
            else
            {
                RenderUstr(driver, t.ToString(), col, line, width, start);
            }
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