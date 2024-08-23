namespace DCCPanelController.Components.TrackComponents;

public partial class SymbolView : ContentView {

    public SymbolView() {
        InitializeComponent();
    }
    public SymbolViewModel ViewModel => (SymbolViewModel)BindingContext;
}