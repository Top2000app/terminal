namespace Top2000.Apps.Teminal.Theme
{
    public class LightTheme : ThemeScope, ITheme
    {
        public Color Top2000Colour => new(218, 22, 28);

        public ColorScheme SelectedEditionLabelColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(227, 142, 153)),
            Normal = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(227, 142, 153)),
            Focus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(227, 142, 153)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(227, 142, 153)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(227, 142, 153)),
        };

        public ColorScheme ListViewColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.Black, background: new(236, 236, 236)),
            Normal = new Terminal.Gui.Attribute(foreground: Color.Black, background: new(236, 236, 236)),
            Focus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new(227, 142, 153)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new(227, 142, 153)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.Black, background: new(227, 142, 153)),
        };

        public ColorScheme MenuBarColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(236, 236, 236)),
            Normal = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(236, 236, 236)),
            Focus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new(227, 142, 153)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new(227, 142, 153)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(236, 236, 236)),
        };

        public ColorScheme TrackInfoColorScheme => new()
        {
            Disabled = new Terminal.Gui.Attribute(foreground: Color.Black, background: Color.White),
            Normal = new Terminal.Gui.Attribute(foreground: Color.Black, background: Color.White),
            Focus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(227, 142, 153)),
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.Black, background: new Color(227, 142, 153)),
            HotNormal = new Terminal.Gui.Attribute(foreground: Color.Black, background: Color.White),
        };
    }
}
