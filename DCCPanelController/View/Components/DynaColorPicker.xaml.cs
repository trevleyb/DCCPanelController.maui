using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;

namespace DCCPanelController.View.Components;

public partial class DynaColorPicker : Popup {

    private Color? _selectedColor;

    public DynaColorPicker(Color color) {
        InitializeComponent();
        SelectedColor = color;
        BindingContext = this;
    }

    public Color? SelectedColor {
        get => _selectedColor;
        set {
            _selectedColor = value;
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private async Task CloseOnSelectedAsync() {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(SelectedColor, cts.Token);
    }
}