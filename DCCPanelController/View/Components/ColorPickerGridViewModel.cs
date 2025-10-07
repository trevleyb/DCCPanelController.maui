using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class ColorPickerGridViewModel : ObservableObject {
    private static readonly IReadOnlyCollection<Color> CachedColors = AppleCrayonColors.Colors;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedColorName))]
    private Color? _selectedColor;

    public ColorPickerGridViewModel() {
        SelectableColors = CachedColors;
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedColor)) {
                OnPropertyChanged(nameof(SelectedColorName));
            }
        };
    }

    public ColorPickerGridViewModel(Color selectedColor) : this() => SelectedColor = selectedColor;
    public IReadOnlyCollection<Color> SelectableColors { get; init; }
    public string SelectedColorName => (SelectedColor == null ? "Default" : AppleCrayonColors.Name(SelectedColor ?? Colors.White)).ToUpper();

    // Add events for the popup
    public event Action<Color?>? ColorSelectionCompleted;
    public event Action? SelectionCancelled;

    [RelayCommand]
    private async Task OnColorSelected(Color color) {
        SelectedColor = color;
    }

    [RelayCommand]
    private async Task OnCancel() => SelectionCancelled?.Invoke();

    [RelayCommand]
    private async Task OnSave() => ColorSelectionCompleted?.Invoke(SelectedColor);
    
    [RelayCommand]
    private async Task OnNone() => ColorSelectionCompleted?.Invoke(Colors.Transparent);

}