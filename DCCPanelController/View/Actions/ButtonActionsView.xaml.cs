using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsView : ContentView {
    public ButtonActionsView(ButtonActions actions, double? width, bool isButtonContext = true) {
        InitializeComponent();
        BindingContext = new ButtonActionsViewModel(actions, isButtonContext);
        if (width is not null) this.WidthRequest = (double)width;
    }
}