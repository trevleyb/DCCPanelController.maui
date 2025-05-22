using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorViewer {
    private readonly ConnectionService? _connectionService;
    private PanelEditorViewerViewModel? _viewModel;

    public PanelEditorViewer(PanelEditorViewerViewModel viewModel, ConnectionService connectionService) {
        InitializeComponent();
        _connectionService = connectionService;
        _viewModel = viewModel;
        _viewModel.ViewerAction += ViewModelOnViewerAction;
        BindingContext = viewModel;
    }

    private async void ViewModelOnViewerAction(object? sender, ViewerEventArgs e) {
        switch (e.Type) {
        case ViewerActionType.Edit:
            if (e.Panel is { } panel) {
                var editorPage = new PanelEditor(panel);
                await Navigation.PushAsync(editorPage);
                await editorPage.PageClosed;
            }
            break;
        default:
            break;
        }
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        if (PanelsLayout == null || PanelsCollectionView == null) return;

        if (DeviceInfo.Platform == DevicePlatform.iOS) {
            if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone) {
                if (width < height) {
                    PanelsLayout.Span = 1;
                    PanelsCollectionView.ItemTemplate = (DataTemplate)this.Resources["HorizontalTemplate"];
                } else {
                    SetWideScreenLayout(width);
                }
            } else {
                SetWideScreenLayout(width);
            }
        } else {
            SetWideScreenLayout(width);
        }
    }

    private void SetWideScreenLayout(double width) {
        PanelsLayout.Span = width switch {
            > 1000 => 4,
            > 800  => 3,
            > 400  => 2,
            _      => 1
        };
        PanelsCollectionView.ItemTemplate = (DataTemplate)this.Resources["VerticalTemplate"];
    }
}