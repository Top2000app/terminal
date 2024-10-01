using Top2000.Apps.Teminal.Custom;
using Top2000.Apps.Teminal.Theme;
using Top2000.Apps.Teminal.Views.About;
using Top2000.Apps.Teminal.Views.SelectEdition;
using Top2000.Apps.Teminal.Views.TrackInformation;
using Top2000.Features.AllEditions;
using Top2000.Features.AllListingsOfEdition;

namespace Top2000.Apps.Teminal.Views;

public class MainWindow : Toplevel
{
    private ITheme theme;
    private readonly IMediator mediator;
    private readonly TrackInformationView trackInformationView;
    private HashSet<TrackListing> trackListings;
    private readonly MenuItem showByPosition;
    private readonly MenuItem showByDate;
    private readonly MenuItem showlightTheme;
    private readonly MenuItem showDarkTheme;
    private readonly FrameView listingFrame;
    private readonly Label selectedEditionLabel;
    private readonly SelectEditionDialog selectEditionDialog;

    public MainWindow(IMediator mediator, TrackInformationView view, HashSet<TrackListing> trackListings, SortedSet<Edition> editions)
    {
        theme = (ITheme)ThemeManager.Instance[ThemeManager.Instance.Theme];
        ThemeManager.Instance.ThemeChanged += ThemeChanged;

        this.mediator = mediator;
        trackInformationView = view;
        this.trackListings = trackListings;
        ColorScheme = Colors.ColorSchemes["Base"];
        SelectedEdition = editions.First();

        showByPosition = new("_Show by position", "", ShowListingsByPosition, canExecute: () => SelectedEdition.HasPlayDateAndTime)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = true,
        };

        showByDate = new("_Show by date", "", ShowByDate, canExecute: () => SelectedEdition.HasPlayDateAndTime)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = false,
        };

        showlightTheme = new("Light theme", "", ChangeToLigthTheme)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = ThemeManager.Instance.Theme == nameof(LightTheme)
        };

        showDarkTheme = new("Dark theme", "", ChangeToDarkTheme)
        {
            CheckType = MenuItemCheckStyle.Radio,
            Checked = ThemeManager.Instance.Theme == nameof(DarkTheme)
        };

        selectEditionDialog = new(editions);

        var menu = new MenuBar
        {
            Menus =
            [
                new MenuBarItem("_File", new MenuItem[] {
                    new("_Selecteer Editie", "", async () => await ShowSelectedEditionDialog() ),
                    null!,
                    new("_Quit", "", () => { Application.RequestStop (); })
                }),
                new MenuBarItem("_View", new MenuItem[] {
                    showByPosition,
                    showByDate,
                    null!,
                    showlightTheme,
                    showDarkTheme
                }),
                new MenuBarItem("_Help", new MenuItem[] {
                    new("_About", "", async () => {await new AboutDialog().ShowDialogAsync(); })
                }),
            ],
            ColorScheme = theme.MenuBarColorScheme
        };

        listingFrame = new()
        {
            X = 0,
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(39),
            Height = Dim.Fill(),
            CanFocus = true,
            ColorScheme = theme.ListViewColorScheme,
            BorderStyle = LineStyle.None,
        };

        selectedEditionLabel = new()
        {
            X = 0,
            Y = 0,
            Height = 1,
            Width = Dim.Fill(),
            Text = editions.First().Year.ToString(),
            TextAlignment = Alignment.Center,
            ColorScheme = theme.SelectedEditionLabelColorScheme,
        };

        ListingListView = new()
        {
            X = 0,
            Y = 1,
            AllowsMultipleSelection = false,
            Height = Dim.Fill(),
            Width = Dim.Fill(),
            OnOpenTrackAsync = HandleOpenTrackAsync,
        };

        listingFrame.Add(selectedEditionLabel, ListingListView);
        infoFrame = new()
        {
            X = Pos.Right(listingFrame),
            Y = Pos.Bottom(menu),
            Width = Dim.Percent(70)! - 10,
            Height = Dim.Fill(),
            ColorScheme = theme.TrackInfoColorScheme,
            BorderStyle = LineStyle.None,
        };

        infoFrame.Add(view);
        Add(menu, listingFrame, infoFrame);
        ShowListingsByPosition();

    }

    private void ThemeChanged(object? sender, ThemeManagerEventArgs e)
    {
        theme = (ITheme)ThemeManager.Instance[ThemeManager.Instance.Theme];
        infoFrame.ColorScheme = theme.TrackInfoColorScheme;
        selectedEditionLabel.ColorScheme = theme.SelectedEditionLabelColorScheme;
        listingFrame.ColorScheme = theme.ListViewColorScheme;
        MenuBar!.ColorScheme = theme.MenuBarColorScheme;

        trackInformationView.ShowInformationWithTheme(theme);
    }

    private void ChangeToLigthTheme()
    {
        ThemeManager.Instance.Theme = nameof(LightTheme);
        showlightTheme.Checked = true;
        showDarkTheme.Checked = false;
    }

    private void ChangeToDarkTheme()
    {
        ThemeManager.Instance.Theme = nameof(DarkTheme);
        showlightTheme.Checked = false;
        showDarkTheme.Checked = true;
    }

    public MultilineListView ListingListView { get; }
    public Edition SelectedEdition { get; set; }
    public FrameView infoFrame { get; }

    private async Task ShowSelectedEditionDialog()
    {
        var newEdition = await selectEditionDialog.ShowDialogAsync(SelectedEdition);

        if (newEdition != SelectedEdition)
        {
            SelectedEdition = newEdition;
            selectedEditionLabel.Text = newEdition.Year.ToString();
            await LoadEditionAsync();
        }
    }

    private async Task HandleOpenTrackAsync(ListingItem selectedItem)
    {
        if (selectedItem.Id is not null)
        {
            await trackInformationView.LoadTrackInformationAsync(selectedItem.Id.Value, theme);
        }
    }

    public async Task LoadEditionAsync()
    {
        trackListings = await mediator.Send(new AllListingsOfEditionRequest { Year = SelectedEdition.Year });

        if (showByDate.Checked.GetValueOrDefault(false) && SelectedEdition.HasPlayDateAndTime)
        {
            ShowByDate();
        }

        if (!SelectedEdition.HasPlayDateAndTime || showByPosition.Checked.GetValueOrDefault(true))
        {
            ShowListingsByPosition();
        }
    }

    private void ShowByDate()
    {
        showByPosition.Checked = false;
        showByDate.Checked = true;

        var listings = trackListings.Reverse()
            .GroupBy(LocalPlayDateAndTime);

        var dates = listings
            .Select(x => x.Key)
            .GroupBy(LocalPlayDate);

        var list = listings.Select(group =>
            new MultilineListItemGrouping(
                new ListingItemGroup(PositionDateTime(group.Key)) { ForeColour = theme.Top2000Colour },
                group.SelectMany(track => new List<ListingItem> {
                    new(track.TrackId, $"{track.Position,-6}{track.Title}"),
                    new(track.TrackId, $"{ToStatusString(track)}{track.Artist}")
                    })
                )
            );

        var itemId = 0;

        var grouped = dates.Select(group =>
            new MultilineListItemGrouping(
                new ListingItemGroup(group.Key.ToLocalTime().ToString("dddd dd MMM")) { ForeColour = theme.Top2000Colour },
                group.SelectMany(date => new List<ListingItem> {
                    new(itemId++, TimeOnly(date), PositionDateTime(date))
                    })
                )
            );


        ListingListView.Source = new DoubleGroupedListViewWrapper(list, grouped);
    }

    public static DateTime LocalPlayDateAndTime(TrackListing listing) => listing.PlayUtcDateAndTime.ToLocalTime();
    private static DateTime LocalPlayDate(DateTime arg) => arg.Date;

    public void ShowListingsByPosition()
    {
        showByPosition.Checked = true;
        showByDate.Checked = false;

        var list = trackListings
            .GroupByPosition()
            .Select(group =>
                new MultilineListItemGrouping(
                    new ListingItemGroup(group.Key) { ForeColour = theme.Top2000Colour },
                    group.SelectMany(track => new List<ListingItem> {
                                new(track.TrackId, $"{track.Position,-6}{track.Title}"),
                                new(track.TrackId, $"{ToStatusString(track)}{track.Artist}")
                        }
                    )
                )
            );

        ListingListView.Source = new MultilineListViewWrapper(list);
    }

    static string ToStatusString(TrackListing track)
    {
        var value = track.Delta;
        var symbol = Symbols.Same;

        if (value is null)
        {
            if (track.IsRecurring)
            {
                symbol = Symbols.BackInList;
            }
            else
            {
                symbol = Symbols.New + " ";
            }
        }
        else
        {
            if (value.Value > 0)
            {
                symbol = Symbols.Up + " ";
            }
            else if (value.Value < 0)
            {
                symbol = Symbols.Down + " ";
            }
        }

        var status = symbol;

        return status + "    ";
    }

    static string PositionDateTime(DateTime utcPlayTime)
    {
        var localTime = utcPlayTime.ToLocalTime();

        var hour = localTime.Hour + 1;
        var date = localTime.ToString("dddd dd MMM HH");

        return $"{date}:00 - {hour}:00";
    }

    static string TimeOnly(DateTime utcPlayTime)
    {
        var localTime = utcPlayTime.ToLocalTime();

        return $"{localTime.Hour}:00 - {localTime.Hour + 1}:00";
    }
}

