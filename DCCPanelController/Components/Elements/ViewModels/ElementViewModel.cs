using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Components.TrackComponents;
using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Components.Elements.ViewModels;

public abstract partial class ElementViewModel : BaseViewModel {
    
    /// <summary>
    /// Details and information on the Element itself. This will include what grid reference it is in
    /// </summary>
    [ObservableProperty]
    private IPanelElement _element;
    
    [ObservableProperty] 
    private ImageSource _image;
    
    [ObservableProperty] 
    private Rect _bounds;

    [ObservableProperty] 
    private bool _isDesignMode;

    protected readonly SymbolDetails _symbolDetails;
    
    /// <inheritdoc/>
    protected ElementViewModel(IPanelElement element, SymbolDetails details) {
        _element = element;
        _image = details.Image;
        _element.SymbolType = details.Key;
        _symbolDetails = details;
    }
}
