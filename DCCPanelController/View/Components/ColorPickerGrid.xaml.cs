using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorPickerGrid : Popup {
    private static readonly IReadOnlyCollection<Color> CachedColors = AppleCrayonColors.Colors;
    
    public ColorPickerGrid(Color color) {
        InitializeComponent();
        SelectableColors = CachedColors; 
        SelectedColor = color;
        BindingContext = this;
    }

    public IReadOnlyCollection<Color> SelectableColors { get; init; }

    private Color? _selectedColor;
    public Color? SelectedColor {
        get => _selectedColor;
        set {
            if (_selectedColor != value) {
                _selectedColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedColorName));
            }
        }
    }

    public string SelectedColorName => (SelectedColor == null ? "Default" : AppleCrayonColors.Name(SelectedColor ?? Colors.White)).ToUpper();

    [RelayCommand]
    private async Task CloseOnSelectedAsync() {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(SelectedColor, cts.Token);
    }

    private void ColorsView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        // Property change notification is now handled in the SelectedColor setter
    }
}