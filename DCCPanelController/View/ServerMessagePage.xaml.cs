using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View;

public partial class ServerMessagesPage : ContentPage {
    public ServerMessagesPage(ServerMessagesViewModel viewModel) {
        BindingContext = viewModel;
        InitializeComponent();

        SafeAreaEdges = SafeAreaEdges.None;
        var safeInsets = On<iOS>().SafeAreaInsets();
        MainStackLayout.Padding = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Right, 0);

        SizeChanged += (_, __) => UpdateWidthState();
        Loaded += (_, __) => UpdateWidthState();    // ensure initial state when Width is valid
        Appearing += (_, __) => UpdateWidthState(); // for safety

    }
    
    void UpdateWidthState()
    {
        if (BindingContext is ServerMessagesViewModel vm) {
            vm.IsWide = Width >= 700;
            vm.IsNarrow = !vm.IsWide;
        }
    }

}