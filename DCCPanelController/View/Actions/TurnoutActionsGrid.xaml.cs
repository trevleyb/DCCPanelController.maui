using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGrid : ContentView {
    public TurnoutActionsGrid(TurnoutActions turnoutPanelActions, ActionsContext context, List<string> availableTurnouts) {
        InitializeComponent();
        BindingContext = new TurnoutActionsGridViewModel(turnoutPanelActions, context, availableTurnouts);
    }

    /// <summary>
    ///     Ideally we would use a Picker, but there is a bug in the picker and it is clearing the Selected Item
    ///     and buggered if I could solve it so I wrote a popup selector.
    /// </summary>
    private async void IdLabel_OnClicked(object? sender, EventArgs e) {
        try {
            if (sender is Button picker) {
                var selectedItem = picker.Text ?? "";
                if (BindingContext is TurnoutActionsGridViewModel viewModel) {
                    var popup = new IDPicker("Turnout", selectedItem, viewModel.SelectableTurnouts(selectedItem));
                    if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
                        var result = await mainpage.ShowPopupAsync(popup);
                        if (result is string resultItem) {
                            picker.Text = resultItem;
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}