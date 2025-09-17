using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Base;

namespace DCCPanelController.View.Properties.PanelProperties;

public partial class PanelPropertyViewModel : BaseViewModel {
    [ObservableProperty] private int    _colorGridSpan = 2; // Default to 2 columns
    [ObservableProperty] private Panel  _panel;
    [ObservableProperty] private string _title;

    public ObservableCollection<ColorItemGroup> GroupedColorSettings { get; set; }
    public ObservableCollection<PanelColorItem> ColorSettings { get; }
    
    public PanelPropertyViewModel(Panel panel) {
        Panel = panel;
        Title = $"{panel.Id} Properties" ?? "Panel Properties";

        ColorSettings = Panel.InitializeColorSettings();
        GroupedColorSettings = Panel.InitializeGroupedColorSettings();
    }

    public Task ApplyChangesAsync() => Task.CompletedTask;

    public ContentView CreatePropertiesView() {
        var propPage = new PanelPropertyPage(this);
        return propPage;
    }

    [RelayCommand]
    private async Task ResetDefaultsClickedAsync() {
        var result = await AskUserToConfirm("Reset Default Colors?", "Are you sure you want to reset all Panels colors to Defaults?");
        if (!result) return; // Exit if the user cancels the delete operation
        Panel.ResetColorsToDefaults();
        OnPropertyChanged(nameof(Panel));
    }

    [RelayCommand]
    private async Task ResetOverridesClickedAsync() {
        var result = await AskUserToConfirm("Reset Overriden Colors?", "This will reset all tracks to the panel colors where they have overriden colors. Do you want to do this?");
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
            foreach (var panel in panels) panel.CopyColorsTo(panel);
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