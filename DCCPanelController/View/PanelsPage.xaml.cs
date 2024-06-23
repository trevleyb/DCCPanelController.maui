using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage {

    PanelsViewModel? _viewModel;
    
    public PanelsPage() {
        InitializeComponent();
        var service = App.ServiceProvider?.GetService<PanelsService>();
        _viewModel = new PanelsViewModel(service);
        BindingContext = _viewModel;
    }
}