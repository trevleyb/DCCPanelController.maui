using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class MainPage : TabbedPage
{
	ConnectionService? _connectionService;
	public MainPage()
	{
		InitializeComponent();
		_ = LoadAndConnect().WaitAsync(new CancellationToken());
	}

	private async Task LoadAndConnect() {
		var settings = App.ServiceProvider?.GetService<SettingsService>();
		if (settings is not null) {
			await settings.Load();
			if (!settings.Settings.DemoMode) {
				try {
					var service = App.ServiceProvider?.GetService<ConnectionService>();
					service?.Connect(settings.Settings.WiServer);
				} catch (Exception ex) {
					var result = await DisplayAlert("Unable to Connect", "Unable to connect to the specified WiThrottle Service.", "DemoMode", "Settings");
					if (result) {
						settings.Settings.DemoMode = true;
						settings.Load();
					} else {
						Navigation.PushAsync(new SettingsPage());
					}
				}
			}
		}


	}
}

