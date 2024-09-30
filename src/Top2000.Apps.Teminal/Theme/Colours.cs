namespace Top2000.Apps.Teminal.Theme
{
    public class Themes
    {
        private readonly ITheme[] themes = [
            new BlackTheme()
            ];

        public ITheme DefaultTheme() => themes[0];
    }

    public interface ITheme
    {
        Color Top2000Colour { get; }
        ColorScheme MenuBarColorScheme { get; }
        ColorScheme ListViewColorScheme { get; }

        ColorScheme TrackInfoColorScheme { get; }
        ColorScheme SelectedEditionLabelColorScheme { get; }
    }

    public class BlackTheme : ITheme
    {
        public Color Top2000Colour => new(218, 22, 28);

        public ColorScheme SelectedEditionLabelColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(135, 8, 24)),
            Normal = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(135, 8, 24)),
            Focus = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(135, 8, 24)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(135, 8, 24)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(135, 8, 24)),
        };

        public ColorScheme ListViewColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(0, 0, 0)),
            Normal = new Terminal.Gui.Attribute(foreground: Color.White, background: new(21, 21, 21)),
            Focus = new Terminal.Gui.Attribute(foreground: Color.White, background: new(135, 8, 24)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.White, background: new(135, 8, 24)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.White, background: new(135, 8, 24)),
        };

        public ColorScheme MenuBarColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.White, background: Color.Black),
            Normal = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(30, 30, 30)),
            Focus = new Terminal.Gui.Attribute(foreground: Color.White, background: new(135, 8, 24)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.White, background: new(135, 8, 24)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(30, 30, 30)),
        };

        public ColorScheme TrackInfoColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.White, background: Color.Black),
            Normal = new Terminal.Gui.Attribute(foreground: Color.White, background: Color.Black),
            Focus = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(135, 8, 24)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(135, 8, 24)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.White, background: new Color(21, 21, 21)),
        };
    }
}
