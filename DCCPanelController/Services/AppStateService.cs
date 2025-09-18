using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.Services.ProfileService;

namespace DCCPanelController.Services;

public partial class AppStateService : ObservableObject, INotifyPropertyChanged {
    private readonly ProfileService.ProfileService _profileService;

    [ObservableProperty] private Color _shellBackgroundColor  = StyleHelper.FromStyle("Primary");
    [ObservableProperty] private Color _shellForegroundColor  = Colors.White;
    [ObservableProperty] private Color _shellUnselectedColor  = Colors.LightGray;
    [ObservableProperty] private Color _tabBarDisabledColor   = Colors.LightGray;
    [ObservableProperty] private Color _tabBarForegroundColor = Colors.White;
    [ObservableProperty] private Color _tabBarTitleColor      = Colors.White;
    [ObservableProperty] private Color _tabBarUnselectedColor = Colors.DarkGray;

    public AppStateService(ProfileService.ProfileService profileService) {
        _profileService = profileService;
        _profileService.ActiveProfileChanged += OnActiveProfileChanged;
        _profileService.ActiveProfileDataChanged += OnActiveProfileDataChanged;
        OnActiveProfileChanged(null, new ProfileChangedEventArgs(null, _profileService.ActiveProfile));
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

    private void OnActiveProfileDataChanged(object? sender, ProfileDataChangedEventArgs e) => SetShellDefault(e.Profile);
    private void OnActiveProfileChanged(object? sender, ProfileChangedEventArgs e) => SetShellDefault(e.NewProfile);

    private void SetShellDefault(Profile? profile) {
        if (_profileService.ActiveProfile == profile) {
            if (profile is { }) {
                Console.WriteLine("Profile Set - Setting Colors");
                ShellBackgroundColor = profile.Settings.BackgroundColor ?? StyleHelper.FromStyle("Primary");
                ShellForegroundColor = profile.Settings.ForegroundColor ?? StyleHelper.FromStyle("White");
                ;

                // Set up the other colors based on what the background color is
                // If it is dark, set the foreground colors light, otherwise set them to be dark. 
                // ------------------------------------------------------------------------------
                if (AppleCrayonColors.IsColorLight(ShellBackgroundColor)) {
                    TabBarTitleColor = Colors.Black;      // Text Color
                    TabBarForegroundColor = Colors.Black; // Icon Color
                    TabBarUnselectedColor = Colors.LightGray;
                    TabBarDisabledColor = Colors.LightSlateGray;
                    ShellUnselectedColor = Colors.LightGrey;
                } else {
                    TabBarTitleColor = Colors.White;      // Text Color
                    TabBarForegroundColor = Colors.White; // Icon Color
                    TabBarUnselectedColor = Colors.DarkGray;
                    TabBarDisabledColor = Colors.DarkSlateGray;
                    ShellUnselectedColor = Colors.DarkGray;
                }
            }
        }
    }
}