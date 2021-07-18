using blastcms.web.MudThemes;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

namespace blastcms.web.Shared
{
	public partial class AppBar
	{
		private bool _isLightMode = false;
		private MudTheme _currentTheme = new MudTheme();

		[Parameter]
		public EventCallback OnSidebarToggled { get; set; }
		[Parameter]
		public EventCallback<MudTheme> OnThemeToggled { get; set; }

		private async Task ToggleTheme()
		{
			_isLightMode = !_isLightMode;

			_currentTheme = !_isLightMode ? new DarkMudTheme() : new LightMudTheme();

			await OnThemeToggled.InvokeAsync(_currentTheme);
		}

		
	}
}
