using DCCPanelController.View;

namespace DCCPanelController;

public partial class App : Application
{
	public static IServiceProvider? ServiceProvider { get; set; }

	public App()
	{
		InitializeComponent();
		
		//
		// _ = LoadAndConnect().WaitAsync(new CancellationToken());
		// private async Task LoadAndConnect() {
		// 	var settings = App.ServiceProvider?.GetService<SettingsService>();
		// 	var settingsViewModel = App.ServiceProvider?.GetService<SettingsViewModel>();
		// 	if (settingsViewModel != null) {
		// 		//await settingsViewModel.RefreshWiServersAsync();
		// 		//await settingsViewModel.ConnectAsync();
		// 	}
		// }

		//MainPage = App.ServiceProvider?.GetService<MainPageFlyOut>();
		MainPage = new MainPageTabbed();

	}
	
}
