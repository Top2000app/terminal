namespace Top2000.Apps.Teminal.Views.TrackInformation;

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
                Driver.SetAttribute(new(new Color(112, 173, 71), cellColor.Background));
            }

            if (i == yellow || i == alsoYellow)
            {
                Driver.SetAttribute(new(new Color(255, 192, 0), cellColor.Background));
            }

            if (i == red)
            {
                Driver.SetAttribute(new(new Color(218, 22, 28), cellColor.Background));
            }

            Driver.AddRune((Rune)render[i]);
            Driver.SetAttribute(cellColor);
        }
    }
}
