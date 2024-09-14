using MediatR;
using Terminal.Gui;
using Top2000.Features.TrackInformation;

namespace Top2000.Apps.Teminal.Views;

public class TrackInformationView : View
{
    private readonly IMediator mediator;

    public TrackInformationView(IMediator mediator)
    {
        this.mediator = mediator;

        this.X = 0;
        this.Y = 0;
        this.Height = Dim.Fill();
        this.Width = Dim.Fill();
    }

    public async Task LoadTrackInformationAsync(int trackId)
    {
        var trackInformation = await this.mediator.Send(new TrackInformationRequest { TrackId = trackId });

        this.Add(new Label()
        {
            X = 0,
            Y = 0,
            Height = 1,
            Width = Dim.Fill(),
            Text = trackInformation.Title,
        });

        this.Add(new Label
        {
            X = 0,
            Y = 1,
            Height = 1,
            Width = Dim.Fill(),
            Text = $"{trackInformation.Artist} ({trackInformation.RecordedYear})",
        });
        //var noteringenText = "Noteringen";

        // this.LineCanvas.AddLine(new Point(0, 3), int.MaxValue, Orientation.Horizontal, LineStyle.Single);

        //this.Add(new Label
        //{
        //    X = 3,
        //    Y = 3,
        //    Height = 1,
        //    Width = noteringenText.Length,
        //    Text = noteringenText
        //});

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
        this.Add(table);



        //this.Add(new Label
        //{
        //    X = 0,
        //    Y = 11,
        //    Width = Dim.Fill(),
        //    Height = 1,
        //    Text = "Sinds ontstaan"
        //});

        //this.Add(new Label
        //{
        //    X = 0,
        //    Y = 12,
        //    Width = Dim.Fill(),
        //    Height = 1,
        //    Text = "In Top 2000"
        //});
    }

}

public class ListingInformationTableView : TableView
{
    public ListingInformationTableView(ListingInformationSource table) : base(table) { }

    protected override void RenderCell(Terminal.Gui.Attribute cellColor, string render, bool isPrimaryCell)
    {
        int green = render.IndexOf("\uFC35", StringComparison.CurrentCultureIgnoreCase);
        int yellow = render.IndexOf("\uF73A", StringComparison.CurrentCultureIgnoreCase);
        int alsoYellow = render.IndexOf("\uF94F", StringComparison.CurrentCultureIgnoreCase);
        int red = render.IndexOf("\uFC2C", StringComparison.CurrentCultureIgnoreCase);

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
        this.listings = trackDetails.Listings;

        this.rowcolumn = this.listings
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



    public object this[int row, int col] => this.rowcolumn[row][col];

    public string[] ColumnNames => ["", "", ""];

    public int Columns => this.ColumnNames.Length;

    public int Rows => this.listings.Count;
}
