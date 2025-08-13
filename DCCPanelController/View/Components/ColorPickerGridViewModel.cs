using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;


namespace DCCPanelController.View.Components;

public partial class ColorPickerGridViewModel : ObservableObject, IQueryAttributable {

    private IPopupService popupService;
    public IReadOnlyCollection<Color> SelectableColors { get; init; }
    private static readonly IReadOnlyCollection<Color> CachedColors = AppleCrayonColors.Colors;

    public string SelectedColorName => (SelectedColor == null ? "Default" : AppleCrayonColors.Name(SelectedColor ?? Colors.White)).ToUpper();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedColorName))]
    private Color? _selectedColor; 

    public ColorPickerGridViewModel(IPopupService popupService) {
        this.popupService = popupService;
        SelectableColors = CachedColors;
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedColor)) {
                OnPropertyChanged(nameof(SelectedColorName));
            }
        };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> queryAttributes) {
        SelectedColor = queryAttributes.TryGetValue("color", out var color) ? color as Color : null;
    }

    [RelayCommand]
    async Task OnColorSelected(Color color) {
        Console.WriteLine($"Color selected: {color}");
    }
    
    [RelayCommand]
    async Task OnCancel() {
        await popupService.ClosePopupAsync(Shell.Current);
    }
    
    [RelayCommand()]
    async Task OnSave() {
        await popupService.ClosePopupAsync(Shell.Current, SelectedColor);
    }
}