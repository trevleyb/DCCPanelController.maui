using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Elements.ViewModels;

namespace DCCPanelController.Components.Elements.Views;

public partial class SymbolView : ContentView {

    public SymbolView() {
        InitializeComponent();
    }
    public SymbolViewModel ViewModel => (SymbolViewModel)BindingContext;
}