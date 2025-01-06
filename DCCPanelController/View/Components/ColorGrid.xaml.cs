using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorGrid : Popup {
    public ColorGrid() {
        InitializeComponent();
        ColorOptions = new ObservableCollection<ColorOption>(PredefinedColors.SelectableColors());
        BindingContext = this;
    }

    public ObservableCollection<ColorOption> ColorOptions { get; private set; }

    private void OnColorSelected(object sender, SelectionChangedEventArgs e) {
        var selected = (e.CurrentSelection.Count > 0 ? e.CurrentSelection[0] : null) as ColorOption;
        if (selected is {Name: "None"}) selected = null;        
        Close(selected);
    }
}