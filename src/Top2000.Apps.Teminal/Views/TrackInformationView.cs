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
        this.Height = Dim.Fill(1);
        this.Width = Dim.Fill(1);
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

        //var table = new TableView(listings)
        //{
        //    X = 0,
        //    Y = 5,
        //    Height = 7,
        //    Width = width,
        //    FullRowSelect = false,
        //};




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

