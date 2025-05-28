using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.Properties.PanelProperties;

public partial class PanelPropertyViewModel : BaseViewModel, IPropertiesViewModel {
    [ObservableProperty] private Panel _panel;
    [ObservableProperty] private string _title;
    [ObservableProperty] private int _colorGridSpan = 2; // Default to 2 columns

    public ObservableCollection<ColorSettingItemViewModel> ColorSettings { get; }
    public PropertyDisplayService.ShowPropertiesType ShowPropertiesType { get; set; }
    
    public PanelPropertyViewModel(Panel panel) {
        Panel = panel;
        Title = $"{panel.Id} Properties" ?? "Panel Properties";

        ColorSettings = new ObservableCollection<ColorSettingItemViewModel>();
        InitializeColorSettings();
    }

    private void InitializeColorSettings() {
        ColorSettings.Clear();

        // Add all your color settings here
        // The string for panelPropertyName should match the exact property name in your Panel class
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Background", nameof(Panel.BackgroundColor), p => p.BackgroundColor, (p, c) => p.BackgroundColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Track Border", nameof(Panel.BorderColor), p => p.BorderColor, (p, c) => p.BorderColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "MainLine Track", nameof(Panel.MainLineColor), p => p.MainLineColor, (p, c) => p.MainLineColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "BranchLine Track", nameof(Panel.BranchLineColor), p => p.BranchLineColor, (p, c) => p.BranchLineColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Diverging Track", nameof(Panel.DivergingColor), p => p.DivergingColor, (p, c) => p.DivergingColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Hidden Track", nameof(Panel.HiddenColor), p => p.HiddenColor, (p, c) => p.HiddenColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Button Border", nameof(Panel.ButtonBorder), p => p.ButtonBorder, (p, c) => p.ButtonBorder = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Button Base", nameof(Panel.ButtonColor), p => p.ButtonColor, (p, c) => p.ButtonColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Button On Border", nameof(Panel.ButtonOnBorder), p => p.ButtonOnBorder, (p, c) => p.ButtonOnBorder = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Button Off Border", nameof(Panel.ButtonOffBorder), p => p.ButtonOffBorder, (p, c) => p.ButtonOffBorder = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Button On", nameof(Panel.ButtonOnColor), p => p.ButtonOnColor, (p, c) => p.ButtonOnColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Button Off", nameof(Panel.ButtonOffColor), p => p.ButtonOffColor, (p, c) => p.ButtonOffColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Continuation Marker", nameof(Panel.ContinuationColor), p => p.ContinuationColor, (p, c) => p.ContinuationColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Terminator", nameof(Panel.TerminatorColor), p => p.TerminatorColor, (p, c) => p.TerminatorColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Occupied", nameof(Panel.OccupiedColor), p => p.OccupiedColor, (p, c) => p.OccupiedColor = c));
        ColorSettings.Add(new ColorSettingItemViewModel(Panel, "Show Path", nameof(Panel.ShowPathColor), p => p.ShowPathColor, (p, c) => p.ShowPathColor = c));
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

    public Task ApplyChangesAsync() {
        return Task.CompletedTask;
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {
        var propPage = new PanelPropertyPage(this);
        return propPage;
    }
}