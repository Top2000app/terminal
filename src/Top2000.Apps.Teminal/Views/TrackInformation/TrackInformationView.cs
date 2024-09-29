using Top2000.Features.TrackInformation;

namespace Top2000.Apps.Teminal.Views.TrackInformation;

public class TrackInformationView : View
{
    private readonly IMediator mediator;

    public TrackInformationView(IMediator mediator)
    {
        this.mediator = mediator;

        X = 0;
        Y = 0;
        Height = Dim.Fill();
        Width = Dim.Fill();
    }

    public async Task LoadTrackInformationAsync(int trackId)
    {

        var trackInformation = await mediator.Send(new TrackInformationRequest { TrackId = trackId });

        var title = new Label()
        {
            X = 0,
            Y = 0,
            Height = 1,
            Width = Dim.Fill(),
            Text = trackInformation.Title,
        };


        var artist = new Label
        {
            X = 0,
            Y = Pos.Bottom(title),
            Height = 1,
            Width = Dim.Fill(),
            Text = trackInformation.Artist,
        };

        var recordingYear = new Label
        {
            X = trackInformation.Artist.Length + 1,
            Y = Pos.Bottom(title),
            Height = 1,
            Width = 7,
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, ColorScheme.Normal.Background)),
            Text = $"({trackInformation.RecordedYear})",
        };

        Add(title, artist, recordingYear);

        var line = new LineView(Orientation.Horizontal)
        {
            X = 0,
            Y = 3
        };

        Add(line);

        var noteringenText = "Noteringen";

        Add(new Label
        {
            X = 5,
            Y = 3,
            Height = 1,
            Width = noteringenText.Length + 2,
            Text = $"\u2528{noteringenText}\u2523"
        });

        Add(new Label
        {
            X = 23,
            Y = 3,
            Height = 1,
            Width = 1,
            Text = "\u2533"
        });

        var lineDown = new LineView(Orientation.Vertical)
        {
            X = 23,
            Y = 4,
        };
        Add(lineDown);

        var frame = new FrameView
        {
            X = Pos.Right(lineDown),
            Y = 5,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
            BorderStyle = LineStyle.None
        };
        Add(frame);



        var labels = new[] { "Aantal sinds onstaan", "Aantal in Top 2000", "", "Hoogste notering", "Laagste notering", "Eerste notering", "Laatste notering" };
        var maxLenght = labels.Max(x => x.Length);

        for (var i = 0; i < labels.Length; i++)
        {
            frame.Add(new Label
            {
                X = 0,
                Y = i,
                Text = labels[i],
                Width = maxLenght + 1,
            });
        }

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 0,
            Width = 5,
            Text = $"{trackInformation.Appearances}/{trackInformation.AppearancesPossible}",
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 1,
            Width = 5,
            Text = $"{trackInformation.Appearances}/{trackInformation.Listings.Count}",
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 3,
            Width = 5,
            Text = $"{trackInformation.Highest.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 3,
            Width = 7,
            Text = $"({trackInformation.Highest.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 4,
            Width = 5,
            Text = $"{trackInformation.Lowest.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 4,
            Width = 7,
            Text = $"({trackInformation.Lowest.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 5,
            Width = 5,
            Text = $"{trackInformation.First.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 5,
            Width = 7,
            Text = $"({trackInformation.First.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 6,
            Width = 5,
            Text = $"{trackInformation.Latest.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 6,
            Width = 7,
            Text = $"({trackInformation.Latest.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 7,
            Width = Dim.Fill(),
            Text = $"{trackInformation.Latest.LocalUtcDateAndTime}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        var table = new ListingInformationTableView(new ListingInformationSource(trackInformation))
        {
            X = 0,
            Y = 5,
            Height = Dim.Fill(),
            Width = 23,
            FullRowSelect = true,

            Style = new TableStyle
            {
                ShowHeaders = false,
                ExpandLastColumn = false,
                ShowHorizontalBottomline = false,
                ShowVerticalCellLines = false,
                ShowHorizontalScrollIndicators = false,
                ShowHorizontalHeaderOverline = false,
                AlwaysShowHeaders = false,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalHeaderLines = false,

            }
        };
        Add(table);

        SetNeedsDisplay();
    }

}

public class ListingInformationTableView : TableView
{
    public ListingInformationTableView(ListingInformationSource table) : base(table) { }

    protected override void RenderCell(Terminal.Gui.Attribute cellColor, string render, bool isPrimaryCell)
    {
        var green = render.IndexOf("\uFC35", StringComparison.CurrentCultureIgnoreCase);
        var yellow = render.IndexOf("\uF73A", StringComparison.CurrentCultureIgnoreCase);
        var alsoYellow = render.IndexOf("\uF94F", StringComparison.CurrentCultureIgnoreCase);
        var red = render.IndexOf("\uFC2C", StringComparison.CurrentCultureIgnoreCase);

        for (var i = 0; i < render.Length; i++)
        {
            if (i == green)
            {
                Driver.SetAttribute(new(Terminal.Gui.Color.BrightGreen, cellColor.Background));
            }

            if (i == yellow || i == alsoYellow)
            {
                Driver.SetAttribute(new(Terminal.Gui.Color.BrightYellow, cellColor.Background));
            }

            if (i == red)
            {
                Driver.SetAttribute(new(Terminal.Gui.Color.BrightRed, cellColor.Background));
            }

            Driver.AddRune((Rune)render[i]);
            Driver.SetAttribute(cellColor);
        }
    }
}

public class ListingInformationSource : ITableSource
{
    private readonly SortedSet<ListingInformation> listings;
    private readonly object[][] rowcolumn;

    public ListingInformationSource(TrackDetails trackDetails)
    {
        listings = trackDetails.Listings;

        rowcolumn = listings
            .Select(x =>
            {
                var offset = x.Status switch
                {
                    ListingStatus.NotAvailable => "",
                    ListingStatus.NotListed => "",
                    ListingStatus.Increased => $"\uFC35 {Math.Abs(x.Offset!.Value)}",
                    ListingStatus.New => "\uF73A",
                    ListingStatus.Unchanged => "\uFA74",
                    ListingStatus.Back => "\uF94F",
                    ListingStatus.Decreased => $"\uFC2C {Math.Abs(x.Offset!.Value)}",
                    _ => "\uFAAF"
                };

                return new object[] { x.Edition.ToString().PadRight(5, ' '), x.Position?.ToString().PadRight(5, ' ') ?? "-", offset };
            })
            .ToArray();
    }



    public object this[int row, int col] => rowcolumn[row][col];

    public string[] ColumnNames => ["", "", ""];

    public int Columns => ColumnNames.Length;

    public int Rows => listings.Count;
}
