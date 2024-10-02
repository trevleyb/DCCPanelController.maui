using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {
   
    [ObservableProperty] private Panel _panel;

    public PanelEditorViewModel(Panel panel) {
        _panel = panel;
    }
}