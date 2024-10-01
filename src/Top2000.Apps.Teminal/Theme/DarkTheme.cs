namespace Top2000.Apps.Teminal.Theme
{
    public class DarkTheme : ThemeScope, ITheme
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
            Focus = new Terminal.Gui.Attribute(foreground: Color.White, background: Color.Black),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.White, background: Color.Black),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.White, background: Color.Black),
        };
    }
}
