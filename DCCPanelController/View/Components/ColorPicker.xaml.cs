using CommunityToolkit.Maui.Views;

namespace DCCPanelController.View.Components;

public partial class ColorPicker : Popup {

    private Color? _selectedColor;

    public ColorPicker(Color color) {
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

    private async void CloseOnSelected(object? sender, EventArgs e) {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(SelectedColor, cts.Token);
    }
}