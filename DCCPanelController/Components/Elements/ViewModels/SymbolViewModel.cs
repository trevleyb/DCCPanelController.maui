using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class SymbolViewModel : ObservableObject {
    [ObservableProperty] private string _key;
    [ObservableProperty] private string _set;
    [ObservableProperty] private string _name;
    [ObservableProperty] private int _width;
    [ObservableProperty] private int _height;
    [ObservableProperty] private ImageSource _image;
    
    /// <summary>
    /// Represents a view model for a symbol. Stores and manages the data associated
    /// with how a Symbol, on the display panel is formed and shown.  
    /// </summary>
    /// <param name="set">What is the set name for this sybol</param>
    /// <param name="name">The name of this item. </param>
    /// <param name="image">The symbol image associated with this item</param>
    /// <param name="width">The number of cells wide this element takes</param>
    /// <param name="height">The number of cells high this element takes</param>
    public SymbolViewModel(string set, string name, ImageSource image, int width, int height) {
        Key = $"{set}:{name}";
        Set = set;
        Name = name;
        Image = image;
        Height = height;
        Width = width;
    }
    public SymbolViewModel(string name, ImageSource image, int width, int height) : this ("default",name,image,width,height) { }
    public SymbolViewModel(string name, ImageSource image) : this ("default", name,image,1,1) { }
}