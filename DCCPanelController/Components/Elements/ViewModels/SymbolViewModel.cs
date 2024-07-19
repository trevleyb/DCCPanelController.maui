using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class SymbolViewModel : ObservableObject {
    [ObservableProperty] private string _key;
    [ObservableProperty] private string _name;
    [ObservableProperty] private ImageSource _image;
    [ObservableProperty] private int _width;
    [ObservableProperty] private int _height;

    
    /// <summary>
    /// Represents a view model for a symbol. Stores and manages the data associated
    /// with how a Symbol, on the display panel is formed and shown.  
    /// </summary>
    /// <param name="key">The reference key for this item - used to recreate</param>
    /// <param name="name">The name of this item. </param>
    /// <param name="image">The symbol image associated with this item</param>
    /// <param name="width">The number of cells wide this element takes</param>
    /// <param name="height">The number of cells high this element takes</param>
    public SymbolViewModel(string key, string name, ImageSource image, int width, int height) {
        Key = key;
        Name = name;
        Image = image;
        Height = height;
        Width = width;
    }
    public SymbolViewModel(string name, ImageSource image, int width, int height) : this (name,name,image,width,height) { }
    public SymbolViewModel(string name, ImageSource image) : this (name,name,image,1,1) { }
}