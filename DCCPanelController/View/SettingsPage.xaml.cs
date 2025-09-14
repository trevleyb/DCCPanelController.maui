using System.ComponentModel;
using DCCPanelController.Clients;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<SettingsPage>  _logger;
    private readonly SettingsPageViewModel? _pageViewModel;

    public SettingsPage(ILogger<SettingsPage> logger, SettingsPageViewModel pageViewModel) {
        _logger = logger;
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        InitializeComponent();

        pageViewModel.SetActiveSettings();
        _pageViewModel.SelectedSegmentIndex = _pageViewModel.Settings?.ClientSettings?.Type switch {
            DccClientType.Simulator  => 0,
            DccClientType.Jmri       => 1,
            DccClientType.WiThrottle => 2,
            _                        => 0,
        };
        _pageViewModel.IsDirty = false;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);
        if (_pageViewModel is { IsDirty: true } vm) await vm.SaveSettingsAsync();
    }

    private void EditConnectionButtonClicked(object? sender, EventArgs e) {
        var content = _pageViewModel?.LoadSettingsPage();
        var size = MauiViewSizeCalculator.CalculateTotalSize(content, Width, Height);
        if (content is { }) {
            BottomSheet.BottomSheetContent = content;
            BottomSheet.ShowGrabber = true;
            BottomSheet.EnableSwiping = true;
            BottomSheet.CollapseOnOverlayTap = true;
            BottomSheet.CollapsedHeight = 0;
            BottomSheet.Background = Colors.WhiteSmoke;
            BottomSheet.State = BottomSheetState.HalfExpanded;
            if (size.Height > 150) {
                BottomSheet.State = BottomSheetState.FullExpanded;
            }
            BottomSheet.IsModal = true;
            BottomSheet.Show();
            _pageViewModel?.IsDirty = true;
        }
    }

    public void BottomSheetOnStateChanged(object? sender, StateChangedEventArgs e) {
        if (e.NewState is BottomSheetState.Hidden or BottomSheetState.Collapsed) {
            if (sender is SfBottomSheet bottomSheet) bottomSheet.Content = null!;
            _pageViewModel?.IsNavigationDrawerOpen = false;
        } else {
            _pageViewModel?.IsNavigationDrawerOpen = true;
        }
    }

    private void IsProfileActiveChanged(object? sender, EventArgs eventArgs) {
        _pageViewModel?.MarkActiveProfileDefault();
        OnPropertyChanged(nameof(_pageViewModel.IsProfileDefault));
    }
}