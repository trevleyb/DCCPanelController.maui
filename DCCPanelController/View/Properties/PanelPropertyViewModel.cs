using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.PropertyPages;

public partial class PanelPropertyViewModel : BaseViewModel {
    [ObservableProperty] private string _propertyName;
    [ObservableProperty] private Panel _panel;

    public PanelPropertyViewModel(Panel panel) {
        Panel = panel;
        PropertyName = panel.Id ?? "Panel Properties";
    }

    [RelayCommand]
    private async Task ResetDefaultsClickedAsync() {
        var result = await AskUserToConfirm("Reset Default Colors?", "Are you sure you want to reset all Panels colors to the Default?");
        if (!result) return; // Exit if the user cancels the delete operation
        Panel.ResetToDefaults();
        OnPropertyChanged(nameof(Panel));
    }

    [RelayCommand]
    private async Task ResetOverridesClickedAsync() {
        var result = await AskUserToConfirm("Reset Overriden Colors?", "This will reset all tracks to the panel colors where thay have overriden colors. Do you want to do this?");
        if (!result) return; // Exit if the user cancels the delete operation

        foreach (var track in Panel.Entities) {
            var trackType = track.GetType();
            var property = trackType.GetProperty("TrackColor", BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.PropertyType == typeof(Color) && property.CanWrite) {
                property.SetValue(track, null);
            }
        }
        OnPropertyChanged(nameof(Panel));
    }

    [RelayCommand]
    private async Task ToAllPanelsClickedAsync() {
        var result = await AskUserToConfirm("Standardize Defaults?", "This will copy these panel colors to all other panels. Do you want to do this?");
        if (!result) return; // Exit if the user cancels the delete operation

        if (Panel.Panels is { } panels) {
            foreach (var panel in panels) panel.CopyTo(panel);
        }
        OnPropertyChanged(nameof(Panel));
    }

    private async Task<bool> AskUserToConfirm(string title, string message) {
        if (App.Current.Windows[0].Page is { } window) {
            var result = await window.DisplayAlert(title, message, "Yes", "No");
            return result;
        }
        return false;
    }
}
