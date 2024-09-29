namespace Top2000.Apps.Teminal.Views.About;

public class AboutDialog : Dialog
{
    private int y = 1;
    private TaskCompletionSource? taskCompletionSource;

    public AboutDialog()
    {
        Width = 50;

        AddText("TOP 2000 Terminal");
        AddText("Version 0.1-prerelease");
        y++;
        AddText($"Copyright (C) 2012-{DateTime.Now.Year} Rick Neeft");
        y++;
        AddText("Contact: rick.neeft@outlook.com");
        AddText("Website: https://www.top2000.app");
        y++;
        AddText("DISCLAIMER: De maker is niet");
        AddText("aanprakelijk noch verantwoordlijk voor");
        AddText("de juistheid van de informatie. Dit is");
        AddText("geen officele applicatie van de");
        AddText("Nederlandse Publieke Omroep");

        var okButton = new Button { Text = "_Sluiten" };
        okButton.Accept += (s, e) =>
        {
            taskCompletionSource!.SetResult();
            Application.RequestStop();
        };

        Buttons = [okButton];
        ButtonAlignment = Alignment.Center;
        ButtonAlignmentModes = AlignmentModes.StartToEnd;
        Height = y + 4;
    }

    private void AddText(string text)
    {
        var leftPad = ((50 - text.Length) / 2) + 1;

        Add(new Label
        {
            X = 0,
            Y = y++,
            Text = text.PadLeft(leftPad + text.Length, ' ')
        });
    }

    public Task ShowDialogAsync()
    {
        this.taskCompletionSource = new TaskCompletionSource();

        Application.Run(this);

        return this.taskCompletionSource.Task;
    }
}