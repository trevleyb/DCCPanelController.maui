using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.Helpers;
using DCCPanelController.View.PropertyPages;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
using UIModalPresentationStyle = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {
    private const bool AllowMultipleSelection = false;
    private const double MinRightPaneWidth = 75;  // Minimum width constraint for the right pane
    private const double MaxRightPaneWidth = 250; // Maximum width constraint for the right pane

    public PanelEditorPage(PanelEditorViewModel viewModel) {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
        
        // Clear any marked paths
        TrackPointsValidator.ClearTrackPaths(viewModel?.Panel?.Tracks ?? null);
        SetEditModeIcon(EditModeEnum.Move);
        SetGridIcon(true);
    }

    private static NavigationService NavigationService => MauiProgram.ServiceHelper.GetService<NavigationService>();
    private PanelEditorViewModel ViewModel { get; }

    private void PanelView_OnTrackPieceChanged(object? sender, ITrack track) {
        ViewModel.TrackPieceChanged();
    }

    private void PanelView_OnTrackPieceTapped(object? sender, ITrack track) {
        if (AllowMultipleSelection == false && ViewModel.Panel.SelectedTracks.Count > 1) {
            PanelView.ClearSelectedTracks();
            PanelView.MarkTrackSelected(track);
        } else {
            if (track.IsSelected) {
                PanelView.MarkTrackUnSelected(track);
            } else {
                PanelView.MarkTrackSelected(track);
            }
        }

        try {
            switch (ViewModel.Panel.SelectedTracks.Count) {
            case > 1: 
                ViewModel.SelectedTrackSymbol = "Multiple Tracks";
                break;
            case  1: 
                var id = "";
                var selected = ViewModel.Panel.SelectedTracks.First();
                if (selected is ITrackID selectedID) id = $" [{selectedID.ID}]";
                ViewModel.SelectedTrackSymbol = $"{selected.Name}{id}";
                break;
            default: 
                ViewModel.SelectedTrackSymbol = "";
                break;
            }
            // ;
            // >
            // 1 => ViewModel.SelectedTrackSymbol"Multiple Tracks",
            //     1   => $"{ViewModel.Panel.SelectedTracks[0].Name}",
            //     _   => ""
            // };
        } catch {
            ViewModel.SelectedTrackSymbol = "";
        }
    }

    private void PanelView_OnTrackPieceDoubleTapped(object? sender, ITrack track) {
        PanelView.ClearSelectedTracks();
        PanelView.MarkTrackSelected(track);
        ShowEditPropertyPage(sender, EventArgs.Empty);
    }

    private void OnSymbolDragStarting(object sender, DragStartingEventArgs e) {
        if (sender is DragGestureRecognizer { BindingContext: ITrackSymbol symbol }) {
            e.Data.Properties.Add("Track", symbol);
            e.Data.Properties.Add("Source", "DisplaySymbol");
        }
    }

    private void ToggleValidation(object? sender, EventArgs e) {
        PanelView.ShowTrackErrors = !PanelView.ShowTrackErrors;
        PanelView.RebuildGrid(true);
    }

    private async void ShowPanelPropertyPage(object? sender, EventArgs e) {
        try {
            await ShowPanelPropertyPageAsync();
        } catch (Exception ex) {
            Trace.WriteLine($"Exception {ex.Message} in Property Pages."); 
        }
    }

    private async Task ShowPanelPropertyPageAsync() {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        
        //await NavigationService.NavigateToPopupWindow(new PanelPropertyPage(ViewModel.Panel));
        // If this is a iPhone, then use a FULL SIZED screen control
        // -------------------------------------------------------------------------------
        if (DeviceInfo.Idiom == DeviceIdiom.Phone && DeviceInfo.Platform == DevicePlatform.iOS) {
            var navigationPage = new NavigationPage(new PanelPropertyPage(ViewModel.Panel));
#if IOS
                navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
#endif
            await mainPage.Navigation.PushModalAsync(navigationPage);
            PanelView?.RebuildGrid(true);
        }
        else if (DeviceInfo.Idiom == DeviceIdiom.Tablet && DeviceInfo.Platform == DevicePlatform.iOS ||
                 DeviceInfo.Platform == DevicePlatform.MacCatalyst) {

            var popupPage = new PanelPropertyPopup(ViewModel.Panel);
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet || DeviceInfo.Platform == DevicePlatform.MacCatalyst) {
                // popupPage.SetPopupSize(400, 600); // Example width/height in pixels
            }
            await this.ShowPopupAsync(popupPage);
            PanelView?.RebuildGrid(true);
        }
        else {
            Debug.WriteLine("Unhandled platform, no UI launched.");
        }
    }

    private async void ShowEditPropertyPage(object? sender, EventArgs e) {
        try {
            await ShowEditPropertyPageAsync();
        } catch (Exception ex) {
            Trace.WriteLine($"Exception {ex.Message} in Property Pages.");
        }
    }

    private async Task ShowEditPropertyPageAsync() {
        if (ViewModel is { HasSelectedTracks: true, CanUsePropertyPage: true }) {
            var track = ViewModel.Panel.SelectedTracks.First();

            // If this is a iPhone, then use a FULL SIZED screen control
            // -------------------------------------------------------------------------------
            if (DeviceInfo.Idiom == DeviceIdiom.Phone && DeviceInfo.Platform == DevicePlatform.iOS) {
                var navigationPage = new NavigationPage(new DynamicPropertyPage(track));
                #if IOS
                navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
                #endif
               
                await Navigation.PushModalAsync(navigationPage, true);
                //PanelView?.RebuildGrid(true);
                track.TrackChanged();
            }
            else if (DeviceInfo.Idiom == DeviceIdiom.Tablet && DeviceInfo.Platform == DevicePlatform.iOS ||
                     DeviceInfo.Platform == DevicePlatform.MacCatalyst) {

                var popupPage = new DynamicPropertyPopup(track);
                if (DeviceInfo.Idiom == DeviceIdiom.Tablet || DeviceInfo.Platform == DevicePlatform.MacCatalyst) {
                    // popupPage.SetPopupSize(400, 600); // Example width/height in pixels
                }
                await this.ShowPopupAsync(popupPage);
                track.TrackChanged();
            }
            else {
                Debug.WriteLine("Unhandled platform, no UI launched.");
            }
            PanelView?.MarkTrackUnSelected(track);
        }
    }

    private void RotateLeft(object? sender, EventArgs e) {
        foreach (var track in ViewModel.Panel.SelectedTracks) track.RotateLeft();
    }

    private void RotateRight(object? sender, EventArgs e) {
        foreach (var track in ViewModel.Panel.SelectedTracks) track.RotateRight();
    }

    private void DeleteTrackPiece(object? sender, EventArgs e) {
        foreach (var track in ViewModel.Panel.SelectedTracks) {
            PanelView.RemoveTrackPiece(track);
        }
    }

    private void SetEditModeIcon(EditModeEnum editMode) {
        PanelView.EditMode = editMode;
        SetEditModeIcon();
    }

    private void SetEditModeIcon() {
        EditModeToolbar.IconImageSource = PanelView.EditMode switch {
            EditModeEnum.Move => "move.png",
            EditModeEnum.Copy => "copy.png",
            EditModeEnum.Size => "crop.png", // Turned off as not worked quite right
            _                 => EditModeToolbar.IconImageSource
        };
    }

    private void ChangeEditMode(object? sender, EventArgs e) {
        PanelView.EditMode = PanelView.EditMode switch {
            EditModeEnum.Move => EditModeEnum.Copy,
            EditModeEnum.Copy => EditModeEnum.Move,
            _                 => PanelView.EditMode
        };

        SetEditModeIcon();
    }

    private void ToggleGrid(object? sender, EventArgs e) {
        PanelView.ShowGrid = !PanelView.ShowGrid;
        SetGridIcon();
    }

    private void SetGridIcon(bool state) {
        PanelView.ShowGrid = state;
        SetGridIcon();
    }

    private void SetGridIcon() {
        GridButton.IconImageSource = PanelView.ShowGrid ? "grid_on.png" : "grid_off.png";
        PanelView.RebuildGrid(true);
    }

    private void ZoomIn_Clicked(object? sender, EventArgs e) {
        if (LeftPane.Scale < 3.0) LeftPane.Scale += 0.1;
    }

    private void ZoomOut_Clicked(object? sender, EventArgs e) {
        if (LeftPane.Scale > 0.5) LeftPane.Scale -= 0.1;
    }

    private void ZoomReset_Clicked(object? sender, EventArgs e) {
        LeftPane.Scale = 1.0;
    }
}