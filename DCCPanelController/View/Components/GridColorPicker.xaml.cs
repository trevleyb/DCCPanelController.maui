using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class GridColorPicker : Popup {
    private Color? _selectedColor;

    public GridColorPicker(Color color) {
        InitializeComponent();
        SelectedColor = color;
        SelectableColors = AppleCrayonColors.Colors;
        BindingContext = this;
    }

    public IReadOnlyCollection<Color> SelectableColors { get; init; }

    public Color? SelectedColor {
        get => _selectedColor;
        set {
            _selectedColor = value;
            OnPropertyChanged();
        }
    }

    public string SelectedColorName => (SelectedColor == null ? "Default" : AppleCrayonColors.Name(SelectedColor ?? Colors.White)).ToUpper();

    [RelayCommand]
    private async Task CloseOnSelectedAsync() {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(SelectedColor, cts.Token);
    }

    private void ColorsView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        OnPropertyChanged(nameof(SelectedColorName));
    }
}