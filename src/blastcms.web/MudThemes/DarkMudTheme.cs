using MudBlazor;


namespace blastcms.web.MudThemes
{
    public class DarkMudTheme : MudTheme
    {
        public DarkMudTheme()
        {
			Palette = new PaletteDark()
			{
				Black = "#27272f",
				Background = "#32333d",
				BackgroundGrey = "#27272f",
				Surface = "#373740",
				TextPrimary = "#ffffffb3",
				TextSecondary = "rgba(255,255,255, 0.50)",
				AppbarBackground = "#27272f",
				AppbarText = "#ffffffb3",
				DrawerBackground = "#27272f",
				DrawerText = "#ffffffb3",
				DrawerIcon = "#ffffffb3"
			};

		}
    }
}
