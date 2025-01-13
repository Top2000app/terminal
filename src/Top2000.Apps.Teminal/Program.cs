using Figgle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Top2000.Apps.Teminal.Theme;
using Top2000.Apps.Teminal.Views;
using Top2000.Apps.Teminal.Views.TrackInformation;
using Top2000.Data.ClientDatabase;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;
using Top2000.Features.SQLite;

var top2000Terminal = FiggleFonts.Standard.Render("TOP2000 Terminal!");
Console.WriteLine(top2000Terminal);

var builder = Host.CreateApplicationBuilder();

builder.Services
    .AddClientDatabase(new DirectoryInfo(Directory.GetCurrentDirectory()))
    .AddFeaturesWithSQLite()
    .AddTransient<TrackInformationView>()
    .AddSingleton<MainWindow>()
    .BuildServiceProvider()
    ;

var app = builder.Build();

var assemblySource = app.Services.GetRequiredService<Top2000AssemblyDataSource>();
var update = app.Services.GetRequiredService<IUpdateClientDatabase>();

Console.WriteLine("Instellen Top2000 database");

await update.RunAsync(assemblySource);

var onlineSource = app.Services.GetRequiredService<OnlineDataSource>();
var updateOnline = app.Services.GetRequiredService<IUpdateClientDatabase>();


Console.WriteLine("Top2000 database updaten");
await updateOnline.RunAsync(onlineSource);

var mediator = app.Services.GetRequiredService<IMediator>();

var editions = await mediator.Send(new AllEditionsRequest()).ConfigureAwait(false);
var listingsResults = await mediator.Send(new AllListingsOfEditionRequest { Year = editions.First().Year });

Application.Init();

ThemeManager.Themes = new Dictionary<string, ThemeScope>
{
    { nameof(LightTheme), new LightTheme() },
    { nameof(DarkTheme), new DarkTheme() }
};

ThemeManager.Instance.Theme = nameof(DarkTheme);

var trackInformationView = app.Services.GetRequiredService<TrackInformationView>();

Application.Run(new MainWindow(mediator, trackInformationView, listingsResults, editions));

Console.Clear();
Console.WriteLine(top2000Terminal);

Application.Shutdown();