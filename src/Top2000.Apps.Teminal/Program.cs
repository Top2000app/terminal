using Figgle;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using Terminal.Gui.Custom;
using Top2000.Apps.Teminal.Views;
using Top2000.Data.ClientDatabase;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;
using Top2000.Features.SQLite;

Console.WriteLine(
    FiggleFonts.Standard.Render("TOP2000 Terminal!"));

var services = new ServiceCollection()
    .AddClientDatabase(new DirectoryInfo(Directory.GetCurrentDirectory()))
    .AddFeaturesWithSQLite()
    .AddTransient<TrackInformationView>()
    .BuildServiceProvider()
    ;

var assemblySource = services.GetRequiredService<Top2000AssemblyDataSource>();
var update = services.GetRequiredService<IUpdateClientDatabase>();

Console.WriteLine("Settings up Top2000 database");

await update.RunAsync(assemblySource);

var onlineSource = services.GetRequiredService<OnlineDataSource>();
var updateOnline = services.GetRequiredService<IUpdateClientDatabase>();

Console.WriteLine("Updating Top2000 database");

await updateOnline.RunAsync(onlineSource);

var mediator = services.GetRequiredService<IMediator>();

var editions = await mediator.Send(new AllEditionsRequest());
var latestEdition = editions.First();
var listings = await mediator.Send(new AllListingsOfEditionRequest { Year = 2022 });

var toShow = new List<string>();

var groups = new HashSet<string>();

foreach (var item in listings)
{
    var group = Position(item, listings.Count);
    if (groups.Add(group))
    {
        toShow.Add("g     " + group);
    }

    toShow.Add("t" + item.TrackId.ToString().PadRight(5, ' ') + item.Position.ToString().PadRight(6, ' ') + item.Title);
    toShow.Add("a" + item.TrackId.ToString().PadRight(5, ' ') + "      " + item.Artist);
}

var top2000DataSource = new Top2000ListViewDatasource(toShow);

Application.Init();
var menu = new MenuBar([
    new MenuBarItem ("_File", new MenuItem [] {
        new("_Quit", "", () => {
            Application.RequestStop ();
        })
    }),
    new MenuBarItem ("_Help", new MenuItem [] {
        new("_About", "", () => {
            Application.RequestStop ();
        })
    }),
]);

var win = new Window()
{
    X = 0,
    Y = 0,
    Width = Dim.Fill(),
    Height = Dim.Fill(),
    Border = new Border
    {
        BorderStyle = BorderStyle.None,
    }
};

var info = new View
{
    X = Pos.Percent(36),
    Y = 0,
    Width = Dim.Fill(),
    Height = Dim.Fill(),
};

//var editionsFrame = new FrameView("Edities")
//{
//    X = 0,
//    Y = 0, 
//    Width = 25,
//    Height = Dim.Fill(0),
//    CanFocus = true,
//    Shortcut = Key.CtrlMask | Key.E
//};

//editionsFrame.Title = $"{editionsFrame.Title} ({editionsFrame.ShortcutTag})";
//editionsFrame.ShortcutAction = () => editionsFrame.SetFocus();

//var editiesList = new ListView
//{
//    X = 0,
//    Y = 0,
//    Width = Dim.Fill(0),
//    Height = Dim.Fill(0),
//    AllowsMarking = false,
//    CanFocus = true,
//};

//editiesList.SetSource(editions.Select(x => x.Year).ToList());

//editionsFrame.Add(editiesList);

//var listFrame = new FrameView("Listing")
//{
//    X = 0,
//    Y = 1,
//    Width = 25,
//    Height = Dim.Fill(),
//    CanFocus = true,
//};
//listFrame.Title = $"{latestEdition.Year} ({listFrame.ShortcutTag})";
//listFrame.ShortcutAction = () => listFrame.SetFocus();

var list = new MultiLineListView()
{
    X = 0,
    Y = 0,
    AllowsMultipleSelection = false,
    Height = Dim.Fill(),
    Width = Dim.Percent(36),
};

list.OpenSelectedItem += List_OpenSelectedItem;
list.SelectedItemChanged += List_OpenSelectedItem;

// void List_SelectedItemChanged(Terminal.Gui.Custom.ListViewItemEventArgs obj) => throw new NotImplementedException();


//listFrame.Add(list);

win.Add(list);
win.Add(info);
//win.Add(editionsFrame);

////list.SetSource(listings.ToList());
////

list.Source = top2000DataSource;



// Add both menu and win in a single call
Application.Top.Add(menu, win);
Application.Run();
Application.Shutdown();
async void List_OpenSelectedItem(Terminal.Gui.Custom.ListViewItemEventArgs obj) 
{
    var selection = obj.Value.ToString() ?? "";
    if (!string.IsNullOrEmpty(selection) && !selection.StartsWith('g'))
    {
        var trackId = int.Parse(selection[1..5].Trim());
        var view = services.GetRequiredService<TrackInformationView>();
        await view.LoadTrackInformationAsync(trackId);

        info.RemoveAll();
        info.Add(view);
    }
}


static string Position(TrackListing listing,int countOfItems)
{
    const int GroupSize = 100;

    if (listing.Position < 100) return "1 - 100";

    if (countOfItems > 2000)
    {
        if (listing.Position >= 2400) return "2400 - 2500";
    }
    else
    {
        if (listing.Position >= 1900) return "1900 - 2000";
    }

    var min = listing.Position / GroupSize * GroupSize;
    var max = min + GroupSize;

    return $"{min} - {max}";
}
