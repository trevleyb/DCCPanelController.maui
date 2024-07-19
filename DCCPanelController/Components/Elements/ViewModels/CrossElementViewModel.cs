using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class CrossElementViewModel : ElementViewModelBase, ITrackElementViewModel {
    public CrossElementViewModel() {
        Image = ImageSource.FromFile("crossing.png");
        Name = "Crossing";
    }
}