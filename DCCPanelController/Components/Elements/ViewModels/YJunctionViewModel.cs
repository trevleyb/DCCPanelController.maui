using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class YJunctionViewModel : ElementViewModelBase, ITrackElementViewModel {
    public YJunctionViewModel() {
        Image = ImageSource.FromFile("yjunction.png");
        Name = "Wye Junction";
    }
}