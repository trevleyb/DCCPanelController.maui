using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class MainPage : TabbedPage
{
	public MainPage() {
		InitializeComponent();
		_ = LoadAndConnect().WaitAsync(new CancellationToken());
	}

	private async Task LoadAndConnect() {
		var settings = App.ServiceProvider?.GetService<SettingsService>();
		var settingsViewModel = App.ServiceProvider?.GetService<SettingsViewModel>();
		if (settingsViewModel != null) {
			//await settingsViewModel.RefreshWiServersAsync();
			//await settingsViewModel.ConnectAsync();
		}
	}
}

