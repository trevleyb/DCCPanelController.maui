using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    private readonly ButtonActionsGridViewModel _viewModel;

    public ButtonActionsGrid(ButtonActions buttonPanelActions, ActionsContext context, List<string> availableButtons) {
        _viewModel = new ButtonActionsGridViewModel(buttonPanelActions, context, availableButtons);
        InitializeComponent();
        BindingContext = _viewModel;
        foreach (var item in buttonPanelActions) {
            Console.WriteLine($"Buttons: {item.Id}");
        }
    }

    /// <summary>
    ///     Ideally we would use a Picker, but there is a bug in the picker and it is clearing the Selected Item
    ///     and buggered if I could solve it so I wrote a popup selector.
    /// </summary>
    private async void IdLabel_OnClicked(object? sender, EventArgs e) {
        // try {
        //     if (sender is Button picker) {
        //         var selectedItem = picker.Text ?? "";
        //         if (BindingContext is ButtonActionsGridViewModel viewModel) {
        //             var popup = new IDPicker("Button", selectedItem, viewModel.SelectableButtons());
        //             if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
        //                 var result = await mainpage.ShowPopupAsync(popup);
        //                 if (result is string resultItem) {
        //                     picker.Text = resultItem;
        //                 }
        //             }
        //         }
        //     }
        // } catch (Exception ex) {
        //     Console.WriteLine(ex.Message);  
        // }
    }
}