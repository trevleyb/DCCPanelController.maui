using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.PropertyPages;

public partial class PanelPropertyViewModel : BaseViewModel {

    [ObservableProperty] private ObservableCollection<ColorReference> _colorReferences = [];
    [ObservableProperty] private Panel _panel;

    public PanelPropertyViewModel(Panel panel) {
        Panel = panel;
        
        // I just could not get the blasted bindings to work correctly so have given up
        // -----------------------------------------------------------------------------------------------------------------
        // ColorReferences.Add(new ColorReference("Background", ()=>panel.BackgroundColor, newColor=>panel.BackgroundColor = newColor));
        // ColorReferences.Add(new ColorReference("Track Border", ()=>panel.BorderColor, newColor=>panel.BorderColor = newColor));
        // ColorReferences.Add(new ColorReference("MainLine Track", ()=>panel.MainLineColor, newColor=>panel.MainLineColor = newColor));
        // ColorReferences.Add(new ColorReference("BranchLine Track", ()=>panel.BranchLineColor, newColor=>panel.BranchLineColor = newColor));
        // ColorReferences.Add(new ColorReference("Diverging Track", ()=>panel.DivergingColor, newColor=>panel.DivergingColor = newColor));
        // ColorReferences.Add(new ColorReference("Hidden Track", ()=>panel.HiddenColor, newColor=>panel.HiddenColor = newColor));
        //
        // ColorReferences.Add(new ColorReference("Button Border", ()=>panel.ButtonBorder, newColor=>panel.ButtonBorder = newColor));
        // ColorReferences.Add(new ColorReference("Button Color", ()=>panel.ButtonColor, newColor=>panel.ButtonColor = newColor));
        // ColorReferences.Add(new ColorReference("Button Off Border", ()=>panel.ButtonOffBorder, newColor=>panel.ButtonOffBorder = newColor));
        // ColorReferences.Add(new ColorReference("Button Off Color", ()=>panel.ButtonOffColor, newColor=>panel.ButtonOffColor = newColor));
        // ColorReferences.Add(new ColorReference("Button On Border", ()=>panel.ButtonOnBorder, newColor=>panel.ButtonOnBorder = newColor));
        // ColorReferences.Add(new ColorReference("Button On Color", ()=>panel.ButtonOnColor, newColor=>panel.ButtonOnColor = newColor));
        //
        // ColorReferences.Add(new ColorReference("Continuation Marker", ()=>panel.ContinuationColor, newColor=>panel.ContinuationColor = newColor));
        // ColorReferences.Add(new ColorReference("Terminator", ()=>panel.TerminatorColor, newColor=>panel.TerminatorColor = newColor));
        // ColorReferences.Add(new ColorReference("Occupied Track", ()=>panel.OccupiedColor, newColor=>panel.OccupiedColor = newColor)); 
        // ColorReferences.Add(new ColorReference("Path Track", ()=>panel.OccupiedColor, newColor=>panel.OccupiedColor = newColor)); 
    }

    [RelayCommand]
    private async Task ResetDefaultsClickedAsync() {
        var result = await AskUserToConfirm("Reset Default Colors?", "Are you sure you want to reset all Panels colors to the Default?");
        if (!result) return; // Exit if the user cancels the delete operation
        Panel.ResetToDefaults();
        OnPropertyChanged();
    }

    [RelayCommand]
    private async Task ResetOverridesClickedAsync() {
        var result = await AskUserToConfirm("Reset Overriden Colors?", "This will reset all tracks to the panel colors where thay have overriden colors. Do you want to do this?");
        if (!result) return; // Exit if the user cancels the delete operation
        foreach (var track in Panel.Tracks) {
            var trackType = track.GetType();
            var property = trackType.GetProperty("TrackColor", BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.PropertyType == typeof(Color) && property.CanWrite) {
                property.SetValue(track, null);
            }
        }
        OnPropertyChanged();
    }

    [RelayCommand]
    private async Task ToAllPanelsClickedAsync() {
        var result = await AskUserToConfirm("Standardize Defaults?", "This will copy these panel colors to all other panels. Do you want to do this?");
        if (!result) return; // Exit if the user cancels the delete operation
        foreach (var panel in Panel.Panels) {
            panel.CopyTo(panel);
        }
        OnPropertyChanged();
    }

    private async Task<bool> AskUserToConfirm(string title, string message) {
        // Replace this code with the appropriate logic to display a confirmation dialog in your app
        if (App.Current.Windows[0].Page is { } window) {
            var result = await window.DisplayAlert(
                title,
                message,
                "Yes", "No"
            );
            return result;
        }
        return false;
    }
}


public partial class ColorReference : ObservableObject {
    
    private readonly Func<Color> _getSelectedColor;
    private readonly Action<Color> _setSelectedColor;
    
    public ColorReference(string label, Func<Color> getSelectedColor, Action<Color> setSelectedColor)
    {
        Label = label;
        _getSelectedColor = getSelectedColor;
        _setSelectedColor = setSelectedColor;
    }
    
    [ObservableProperty] private string _label;
    
    public Color SelectedColor {
        get => _getSelectedColor();
        set {
            if (!EqualityComparer<Color>.Default.Equals(_getSelectedColor(), value)) {
                _setSelectedColor(value);
                OnPropertyChanged();
            }
        }
    }
}
