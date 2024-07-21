namespace DCCPanelController.Components.Symbols;

public partial class SymbolView : ContentView {

    public SymbolView() {
        InitializeComponent();
    }
    public SymbolViewModel ViewModel => (SymbolViewModel)BindingContext;
}