using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.Base;

public partial class ElementViewModelBase : BaseViewModel, IElementViewModel {
    
    /// <summary>
    /// The 'Name' represents the name or type of this element. This is used to know what is where and to
    /// re-create one when we re-load the model from storage. The name is defined and set in the ViewModel. 
    /// </summary>
    [ObservableProperty] 
    private string _name = "unknown";       

    /// <summary>
    /// If there is an image associated with this item, then it is stored here. 
    /// </summary>
    [ObservableProperty] 
    private ImageSource? _image;
    
    /// <summary>
    /// If there is any text information associated
    /// </summary>
    [ObservableProperty] 
    private string? _text;

    /// <summary>
    /// Details and information on the Element itself. This will include what grid reference it is in
    /// </summary>
    [ObservableProperty]
    private PanelElement? _element;

    /// <summary>
    /// Details and information on the Element itself. This will include what grid reference it is in
    /// </summary>
    [ObservableProperty]
    private Coordinate _coordinates;
    
    [ObservableProperty] 
    private Rect _bounds;

}
