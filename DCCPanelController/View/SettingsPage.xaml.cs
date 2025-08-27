using System.ComponentModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Storage;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.MauiMauiView.Helpers;
using DCCPanelController.Models.DataModel.Repository;
using DCCPanelController.View.Components;
using DCCPanelController.View.Settings;
using DCCPanelController.View.Settings.Jmri;
using DCCPanelController.View.Settings.Simulator;
using DCCPanelController.View.Settings.WiThrottle;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;
using SelectionChangedEventArgs = Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {

    private readonly ILogger<SettingsPage> _logger;
    private readonly SettingsPageViewModel? _pageViewModel;

    public SettingsPage(ILogger<SettingsPage> logger, SettingsPageViewModel pageViewModel) {
        _logger = logger;
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        PropertyChanged += OnPropertyChanged;
        InitializeComponent();

        pageViewModel.SetActiveSettings();
        _pageViewModel.SelectedSegmentIndex = _pageViewModel.Settings?.ClientSettings?.Type switch {
            DCCPanelController.Clients.DccClientType.Simulator  => 0,
            DCCPanelController.Clients.DccClientType.Jmri       => 1,
            DCCPanelController.Clients.DccClientType.WiThrottle => 2,
            _                                                   => 0
        };
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    private void EditConnectionButtonClicked(object? sender, EventArgs e) {
        var content = _pageViewModel?.LoadSettingsPage();
        var size = MauiViewSizeCalculator.CalculateTotalSize(content,Width,Height);
        if (content is not null) {
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
        _pageViewModel?.MarkActiveProfileDefault();;
        OnPropertyChanged(nameof(_pageViewModel.IsProfileDefault));
    }
}