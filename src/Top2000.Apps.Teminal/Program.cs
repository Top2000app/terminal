using System.Collections;
using System.Collections.Immutable;
using System.Linq;
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
var listings = await mediator.Send(new AllListingsOfEditionRequest(editions.First().Year));

var toShow = listings
    .Select(x => new ViewableTrackListing
    {
        Input = x.Position.ToString().PadRight(6, ' ') + x.Title + Environment.NewLine + "      " + x.Artist
    })
    .ToList();

var try1 = new ListingDataSource(listings);

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
    //Width = Dim.Fill(),
    //Height = Dim.Fill() - 1
};

var list = new MultiLineListView()
{
    X = 1,
    Y = 1,
    AllowsMultipleSelection = false,
    Height = Dim.Fill(),
    Width = Dim.Fill(1),
};

win.Add(list);

//list.SetSource(listings.ToList());
//

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

public class ListingDataSource : IListDataSource
{
    private readonly List<TrackListing> items;

    public ListingDataSource(ImmutableHashSet<TrackListing> items)
    {
        this.items = items.ToList();
    }

    public int Count => items.Count * 2;

    public int Length => items.Count * 2;

    public bool IsMarked(int item) => false;
    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        container.Move(col, item * 2);
        var t = items[item];
        //RenderUstr(driver, t.Title, col, line, width);

        var firstLine = t.Position.ToString().PadRight(6, ' ') + t.Title;
        var secondLine = "      " + t.Artist;

        driver.AddStr(firstLine.PadRight(width));
        container.Move(col, item * 2 + 1);
        driver.AddStr(secondLine.PadRight(width));
        //line++;
        //container.Move(col, line+1);

        //col = 0;
        //driver.Move(col, line + 1);
        //driver.AddStr(t.Artist);
        //RenderUstr(driver, t.Artist, col, line, width);
    }

    public void SetMark(int item, bool value) 
    {
        // not supported
    }
    public IList ToList() => items;
}

public class ViewableTrackListing
{
    public string Input { get; set; }

    public override string ToString()
    {
        return Input;
    }
}


class MultiLineListView : ListView
{
    public MultiLineListView() : base(new string[] { }) { }

    //// Override the Redraw method to customize item rendering
    //public override void Redraw(Rect bounds)
    //{
    //    base.Redraw(bounds);

    //    for (int i = 0; i < Source.Count; i++)
    //    {
    //        if (i >= region.Y && i < bounds.Y + bounds.Height)
    //        {
    //            var item =(TrackListing) Source.ToList()[i]!;

    //           // var pos = region.X;

    //            // Render the first line
    //            Move(0, 2 * (i - bounds.Y));
    //            var firstLine = item.Title;
    //            Driver.AddStr(firstLine.PadRight(bounds.Width));

    //            Move(0, 2 * (i - bounds.Y) + 1);
    //            var secondLine = item.Artist;
    //            Driver.AddStr(secondLine.PadRight(bounds.Width));
    //        }
    //    }
    //}

    public override void Redraw(Rect bounds)
    {
        var current = ColorScheme.Focus;
        Driver.SetAttribute(current);
        Move(0, 0);
        var f = Frame;
        var item = base.TopItem;
        bool focused = HasFocus;
        int col = base.AllowsMarking ? 2 : 0;
        int start = 0;

        for (int row = 0; row < f.Height * 2; row++, item++)
        {
            bool isSelected = item == (base.SelectedItem * 2);

            var newcolor = focused ? (isSelected ? ColorScheme.Focus : GetNormalColor())
                           : (isSelected ? ColorScheme.HotNormal : GetNormalColor());

            if (newcolor != current)
            {
                Driver.SetAttribute(newcolor);
                current = newcolor;
            }

            Move(0, row);
            if (Source == null || item >= Source.Count)
            {
                for (int c = 0; c < f.Width; c++)
                    Driver.AddRune(' ');
            }
            else
            {
                var rowEventArgs = new ListViewRowEventArgs(item);
                OnRowRender(rowEventArgs);
                if (rowEventArgs.RowAttribute != null && current != rowEventArgs.RowAttribute)
                {
                    current = (Terminal.Gui.Attribute)rowEventArgs.RowAttribute;
                    Driver.SetAttribute(current);
                }
                //if (allowsMarking)
                //{
                //    Driver.AddRune(source.IsMarked(item) ? (AllowsMultipleSelection ? Driver.Checked : Driver.Selected) :
                //        (AllowsMultipleSelection ? Driver.UnChecked : Driver.UnSelected));
                //    Driver.AddRune(' ');
                //}
                Source.Render(this, Driver, isSelected, item, col, row, f.Width - col, start);
            }
        }
    }

    // Override the Height property to reflect that each item takes 2 rows
    //   public new int Height => 2 * Source.Count;
}