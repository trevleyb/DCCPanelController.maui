using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsView : ContentView {
    public ButtonActionsView(ButtonActions actions, Dictionary<string,string> availableButtons, bool isButtonContext = true) {
        InitializeComponent();
        BindingContext = new ButtonActionsViewModel(actions, availableButtons, isButtonContext);
    }
}