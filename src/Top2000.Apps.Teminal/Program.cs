using Figgle;
using Microsoft.Extensions.DependencyInjection;
using Top2000.Apps.Teminal.Views;
using Top2000.Apps.Teminal.Views.TrackInformation;
using Top2000.Data.ClientDatabase;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;
using Top2000.Features.SQLite;

var top2000Terminal = FiggleFonts.Standard.Render("TOP2000 Terminal!");
Console.WriteLine(top2000Terminal);

var services = new ServiceCollection()
    .AddClientDatabase(new DirectoryInfo(Directory.GetCurrentDirectory()))
    .AddFeaturesWithSQLite()
    .AddTransient<TrackInformationView>()
    .AddSingleton<MainWindow>()
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

var editions = await mediator.Send(new AllEditionsRequest()).ConfigureAwait(false);
var listingsResults = await mediator.Send(new AllListingsOfEditionRequest { Year = editions.First().Year });

Application.Init();

var trackInformationView = services.GetRequiredService<TrackInformationView>();

Application.Run(new MainWindow(mediator, trackInformationView, listingsResults, editions));

Console.Clear();
Console.WriteLine(top2000Terminal);

Application.Shutdown();
