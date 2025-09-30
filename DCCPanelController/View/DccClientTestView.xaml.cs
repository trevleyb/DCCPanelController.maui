using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;

namespace DCCPanelController.View;

public partial class DccClientTestView : ContentPage
{
    public DccClientTestView(ProfileService prf, ConnectionService svc) {
        BindingContext = new DccClientTestViewModel(prf,svc);
        InitializeComponent();
    }
}