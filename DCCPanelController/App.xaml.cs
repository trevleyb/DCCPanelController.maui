using DCCPanelController.View;

namespace DCCPanelController;

public partial class App : Application
{
	public static IServiceProvider? ServiceProvider { get; set; }

	public App()
	{
		InitializeComponent();
		Routing.RegisterRoute("PanelsPage", typeof(View.PanelsPage));
		Routing.RegisterRoute("PanelDetailsPage", typeof(View.PanelDetailsPage));
		//MainPage = new AppShell();
		MainPage = new MainPage();
	}
}
