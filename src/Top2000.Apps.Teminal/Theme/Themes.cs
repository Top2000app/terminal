namespace Top2000.Apps.Teminal.Theme
{
    public class Themes
    {
        private readonly ITheme[] themes = [
            new DarkTheme(),
            new LightTheme()
            ];

        public ITheme DefaultTheme() => themes[1];
    }
}
