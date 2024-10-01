using Top2000.Apps.Teminal.Theme;
using Top2000.Features.TrackInformation;

namespace Top2000.Apps.Teminal.Views.TrackInformation;

public class TrackInformationView : View
{
    private readonly IMediator mediator;

    public TrackInformationView(IMediator mediator)
    {
        this.mediator = mediator;

        X = 1;
        Y = 0;
        Height = Dim.Fill();
        Width = Dim.Fill();
    }

    public void ShowInformationWithTheme(ITheme theme)
    {
        if (TrackDetails is null)
        {
            return;
        }

        var title = new Label()
        {
            X = 0,
            Y = 0,
            Height = 1,
            Width = Dim.Fill(),
            Text = TrackDetails.Title,
        };

        var artist = new Label
        {
            X = 0,
            Y = Pos.Bottom(title),
            Height = 1,
            Width = Dim.Fill(),
            Text = TrackDetails.Artist,
        };

        var recordingYear = new Label
        {
            X = TrackDetails.Artist.Length + 1,
            Y = Pos.Bottom(title),
            Height = 1,
            Width = 7,
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(theme.Top2000Colour, ColorScheme.Normal.Background)),
            Text = $"({TrackDetails.RecordedYear})",
        };

        Add(title, artist, recordingYear);

        Add(new LineView(Orientation.Horizontal)
        {
            X = 0,
            Y = 2
        });

        var noteringenText = "Noteringen";

        Add(new Label
        {
            X = 0,
            Y = 3,
            Height = 1,
            Width = noteringenText.Length + 2,
            Text = $"{noteringenText}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(theme.Top2000Colour, ColorScheme.Normal.Background)),
        });

        Add(new LineView(Orientation.Horizontal)
        {
            X = 0,
            Y = 4
        });


        var frame = new FrameView
        {
            X = 24,
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
            Text = $"{TrackDetails.Appearances}/{TrackDetails.AppearancesPossible}",
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 1,
            Width = 5,
            Text = $"{TrackDetails.Appearances}/{TrackDetails.Listings.Count}",
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 3,
            Width = 5,
            Text = $"{TrackDetails.Highest.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(theme.Top2000Colour, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 3,
            Width = 7,
            Text = $"({TrackDetails.Highest.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 4,
            Width = 5,
            Text = $"{TrackDetails.Lowest.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(theme.Top2000Colour, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 4,
            Width = 7,
            Text = $"({TrackDetails.Lowest.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 5,
            Width = 5,
            Text = $"{TrackDetails.First.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(theme.Top2000Colour, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 5,
            Width = 7,
            Text = $"({TrackDetails.First.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 1,
            Y = 6,
            Width = 5,
            Text = $"{TrackDetails.Latest.Position}",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(theme.Top2000Colour, ColorScheme.Normal.Background)),
        });

        frame.Add(new Label
        {
            X = maxLenght + 6,
            Y = 6,
            Width = 7,
            Text = $"({TrackDetails.Latest.Edition})",
            ColorScheme = new ColorScheme(new Terminal.Gui.Attribute(Terminal.Gui.Color.Gray, ColorScheme.Normal.Background)),
        });

        if (TrackDetails.Latest.LocalUtcDateAndTime.HasValue)
        {
            frame.Add(new Label
            {
                X = 0,
                Y = labels.Length + 2,
                Text = "Laatste Top 2000",
            });

            var hour = TrackDetails.Latest.LocalUtcDateAndTime.Value.Hour;

            frame.Add(new Label
            {
                X = 0,
                Y = labels.Length + 3,
                Width = Dim.Fill(),
                Text = $"{TrackDetails.Latest.LocalUtcDateAndTime.Value.ToString("dddd dd MMMM yyyy")} {hour}:00 - {hour + 1}:00",
            });
        }

        var table = new ListingInformationTableView(new ListingInformationSource(TrackDetails))
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

    public TrackDetails? TrackDetails { get; set; }

    public async Task LoadTrackInformationAsync(int trackId, ITheme theme)
    {
        TrackDetails = await mediator.Send(new TrackInformationRequest { TrackId = trackId });

        ShowInformationWithTheme(theme);
    }
}
