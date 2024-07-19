using DCCPanelController.Components.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class TerminatorElementViewModel : ElementViewModelBase, ITrackElementViewModel {
    public TerminatorElementViewModel() {
        Image = ImageSource.FromFile("terminate.png");
        Name = "Track Terminator";
    }
}