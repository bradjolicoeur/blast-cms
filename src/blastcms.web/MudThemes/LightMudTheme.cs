using MudBlazor;


namespace blastcms.web.MudThemes
{
    public class LightMudTheme : MudTheme
    {
        public LightMudTheme()
        {
            Palette = new PaletteLight
            {
                AppbarBackground = Colors.Grey.Lighten1
            };
        }
    }
}
