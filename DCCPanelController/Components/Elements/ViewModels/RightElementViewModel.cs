using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class RightElementViewModel : ElementViewModelBase, ITrackElementViewModel{
    public RightElementViewModel() {
        Image = ImageSource.FromFile("angleright.png");
        Name = "Right Track";
    }
}