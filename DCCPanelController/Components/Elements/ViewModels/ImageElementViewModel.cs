using DCCPanelController.Components.TrackComponents;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class ImageElementViewModel : ElementViewModel, IElementViewModel {
   public ImageElementViewModel(ImagePanelElement element, SymbolDetails details) : base (element,details) { }
}
