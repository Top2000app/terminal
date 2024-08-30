using System.Data;
using System.Text;
using MediatR;
using MoreLinq.Extensions;
using Terminal.Gui;
using Terminal.Gui.Graphs;
using Top2000.Features.TrackInformation;

namespace Top2000.Apps.Teminal.Views;

public class TrackInformationView : View
{
    private readonly IMediator mediator;
    private LineCanvas grid;

    public TrackInformationView(IMediator mediator)
    {
        this.mediator = mediator;
        
        this.X = 0;
        this.Y = 0;
        this.Height = Dim.Fill();
        this.Width = Dim.Fill()-1;
        this.grid = new LineCanvas();
    }

    public async Task LoadTrackInformationAsync(int trackId)
    {
        var trackInformation = await mediator.Send(new TrackInformationRequest { TrackId = trackId });

        var listingDataTable = new DataTable();

        listingDataTable.Columns.AddRange(trackInformation.Listings.Select(x => new DataColumn
        {
            ColumnName = x.Edition.ToString(),
            ReadOnly = true,
            DataType = typeof(string),
            Unique = false
        }).ToArray());


        var row0 = listingDataTable.NewRow();
        var row1 = listingDataTable.NewRow();
        var row2 = listingDataTable.NewRow();

        foreach (var item in trackInformation.Listings)
        {
            row0[item.Edition.ToString()] = item.Edition;
            row1[item.Edition.ToString()] = item.Offset?.ToString() ?? "-";
            row2[item.Edition.ToString()] = item.Position?.ToString() ?? "-";
        }

        listingDataTable.Rows.Add(row0);
        listingDataTable.Rows.Add(row1);
        listingDataTable.Rows.Add(row2);

        var listingYearsBuilder = new StringBuilder();
        var offsetBuilder = new StringBuilder();
        var positionBuilder = new StringBuilder();

        foreach (var item in trackInformation.Listings)
        {
            listingYearsBuilder.Append(item.Edition.ToString().PadRight(5, ' '));
            offsetBuilder.Append((item.Offset?.ToString() ?? "").PadRight(5, ' '));
            positionBuilder.Append((item.Position?.ToString() ?? "").PadRight(5, ' '));
        }


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
        var noteringenText = "Noteringen";

        grid.AddLine(new Point(0, 3), int.MaxValue, Orientation.Horizontal, BorderStyle.Single);

        this.Add(new Label
        {
            X = 3,
            Y = 3,
            Height = 1,
            Width = noteringenText.Length,
            Text = noteringenText
        });

        //var listings = new ListView
        //{
        //    X = 0,
        //    Y = 5,
        //    Width = Dim.Fill(),
        //    Height = 3,
        //    AllowsMultipleSelection = false,
        //    AllowsMarking = false,
        //};

        //listings.SetSource(new List<string>
        //{
        //    listingYearsBuilder.ToString(),
        //    offsetBuilder.ToString(),
        //    positionBuilder.ToString()
        //});

        var table = new TableView
        {
            X = 0,
            Y = 5,
            Width = Dim.Fill(),
            Height = 5,
            FullRowSelect = true,
            Data = listingDataTable
        };

        this.Add(table);

        this.Add(new Label
        {
            X = 0,
            Y = 11,
            Width = Dim.Fill(),
            Height = 1,
            Text = "Sinds ontstaan"
        });

        this.Add(new Label
        {
            X = 0,
            Y = 12,
            Width = Dim.Fill(),
            Height = 1,
            Text = "In Top 2000"
        });
    }

    public override void Redraw(Rect bounds) {

        foreach (var p in grid.GenerateImage(bounds))
        {
            this.AddRune(p.Key.X, p.Key.Y, p.Value);
        }
        base.Redraw(bounds);

    }
}
