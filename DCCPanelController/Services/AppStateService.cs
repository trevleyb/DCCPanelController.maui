using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View;
using DCCPanelController.View.Converters;

namespace DCCPanelController.Services;

public partial class AppStateService : ObservableObject, INotifyPropertyChanged {
    private readonly ProfileService.ProfileService _profileService;

    [ObservableProperty] private Color _shellBackgroundColor  = StyleHelper.FromStyle("Primary");
    [ObservableProperty] private Color _shellForegroundColor  = Colors.White;
    [ObservableProperty] private Color _shellUnselectedColor  = Colors.GhostWhite;

    [ObservableProperty] private Color _tabBarTitleColor      = StyleHelper.FromStyle("Primary");
    [ObservableProperty] private Color _tabBarUnselectedColor = Colors.DarkGray;
    [ObservableProperty] private Color _tabBarBackgroundColor = StyleHelper.FromStyle("Primary");
    [ObservableProperty] private Color _tabBarForegroundColor = StyleHelper.FromStyle("Primary");
    [ObservableProperty] private Color _tabBarDisabledColor   = Colors.LightGray;
    
    [ObservableProperty] private bool  _showEditOptions       = true;
    [ObservableProperty] private bool  _hideEditOptions       = false;
    [ObservableProperty] private bool  _debugMode             = false;

    public AppStateService(ProfileService.ProfileService profileService) {
        _profileService = profileService;
        _profileService.ActiveProfileChanged += OnActiveProfileChanged;
        _profileService.ActiveProfileDataChanged += OnActiveProfileDataChanged;
        OnActiveProfileChanged(null, new ProfileChangedEventArgs(null, _profileService.ActiveProfile));
        DebugMode = false;
        #if DEBUG
        DebugMode = true;
        #endif
    }

    public static AppStateService Instance => MauiProgram.ServiceHelper.GetService<AppStateService>();

    public ITile? SelectedTile {
        get;
        set {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            if (field is { }) {
                SelectedTileSet?.Invoke(field);
            } else {
                SelectedTileCleared?.Invoke();
            }
        }
    }

    public bool IsNavigationAllowed => !IsEditingPanel;

    public bool IsEditingPanel {
        get;
        set {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNavigationAllowed));
        }
    }

    public event Action<ITile>? SelectedTileSet;
    public event Action? SelectedTileCleared;

    private void OnActiveProfileDataChanged(object? sender, ProfileDataChangedEventArgs e) {
        SetShellDefault(e.Profile);
    }

    private void OnActiveProfileChanged(object? sender, ProfileChangedEventArgs e) {
        SetShellDefault(e.NewProfile);
    }

    private void SetShellDefault(Profile? profile) {
        if (_profileService.ActiveProfile == profile) {
            if (profile is { }) {
                ShellBackgroundColor = profile.Settings.BackgroundColor ?? StyleHelper.FromStyle("Primary");
                ShellForegroundColor = profile.Settings.ForegroundColor ?? StyleHelper.FromStyle("White");

                var constrastCvt = new ContrastToBackgroundConverter();

                // TabBar background stays same as Shell background
                TabBarBackgroundColor = ShellBackgroundColor;

                // Now contrast all TabBar colors against the TabBar background
                TabBarForegroundColor   = constrastCvt.Convert(StyleHelper.FromStyle("Primary"), typeof(Color), TabBarBackgroundColor, CultureInfo.CurrentCulture) as Color ?? Colors.White;
                TabBarTitleColor        = TabBarForegroundColor;  // Selected tab title uses same as selected icon
                TabBarUnselectedColor   = constrastCvt.Convert(Colors.Gray, typeof(Color), TabBarBackgroundColor, CultureInfo.CurrentCulture) as Color ?? Colors.Gray;
                TabBarDisabledColor     = constrastCvt.Convert(Colors.LightGray, typeof(Color), TabBarBackgroundColor, CultureInfo.CurrentCulture) as Color ?? Colors.LightGray;

                // Shell unselected color contrasted against Shell background
                ShellUnselectedColor    = constrastCvt.Convert(Colors.Gray, typeof(Color), ShellBackgroundColor, CultureInfo.CurrentCulture) as Color ?? Colors.Gray;
            }
        }
    }
}