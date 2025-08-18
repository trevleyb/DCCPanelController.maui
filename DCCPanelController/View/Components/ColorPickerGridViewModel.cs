using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;


namespace DCCPanelController.View.Components;

public partial class ColorPickerGridViewModel : ObservableObject {
    public IReadOnlyCollection<Color> SelectableColors { get; init; }
    private static readonly IReadOnlyCollection<Color> CachedColors = AppleCrayonColors.Colors;
    public string SelectedColorName => (SelectedColor == null ? "Default" : AppleCrayonColors.Name(SelectedColor ?? Colors.White)).ToUpper();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedColorName))]
    private Color? _selectedColor; 

    // Add events for the popup
    public event Action<Color?>? ColorSelectionCompleted;
    public event Action? SelectionCancelled;

    public ColorPickerGridViewModel() {
        SelectableColors = CachedColors;
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedColor)) {
                OnPropertyChanged(nameof(SelectedColorName));
            }
        };
    }

    public ColorPickerGridViewModel(Color selectedColor) : this() {
        SelectedColor = selectedColor;
    }

    [RelayCommand]
    async Task OnColorSelected(Color color) {
        SelectedColor = color;
        Console.WriteLine($"Color selected: {color}");
    }
    
    [RelayCommand]
    async Task OnCancel() {
        SelectionCancelled?.Invoke();
    }
    
    [RelayCommand()]
    async Task OnSave() {
        ColorSelectionCompleted?.Invoke(SelectedColor);
    }
}