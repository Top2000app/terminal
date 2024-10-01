namespace Top2000.Apps.Teminal.Theme
{
    public interface ITheme
    {
        Color Top2000Colour { get; }
        ColorScheme MenuBarColorScheme { get; }
        ColorScheme ListViewColorScheme { get; }

        ColorScheme TrackInfoColorScheme { get; }
        ColorScheme SelectedEditionLabelColorScheme { get; }
    }
}
