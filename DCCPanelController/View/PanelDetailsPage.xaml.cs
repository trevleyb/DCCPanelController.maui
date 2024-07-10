using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View;

public partial class PanelDetailsPage : ContentPage {
    private readonly PanelsDetailsViewModel _viewModel;
    
    public PanelDetailsPage(Panel panel) {
        InitializeComponent();
        _viewModel = new PanelsDetailsViewModel(panel, this);
        SelectedImage.SizeChanged += SelectedImageOnSizeChanged;
        BindingContext = _viewModel;
    }

    private void OnImageTapped(object sender, TappedEventArgs e)
    {
        var touchPosition = e.GetPosition(SelectedImage);
        // if (touchPosition.HasValue) {
        //     var x = (int)touchPosition.Value.X;
        //     var y = (int)touchPosition.Value.Y;
        //     
        //     var gridRef = _viewModel.GetGridReference(x, y);
        //     var gridCrd = _viewModel.GetGridCoordinates(gridRef);
        //     Console.WriteLine($"Image tapped at X: {x}, Y: {y} ==> {gridRef} ==> {gridCrd.x}, {gridCrd.y}");
        //     
        // }
    }

    private void OnImageGridTapped(object sender, TappedEventArgs e)
    {
        var touchPosition = e.GetPosition(SelectedImageGrid);
        // if (touchPosition.HasValue) {
        //     var x = (int)touchPosition.Value.X;
        //     var y = (int)touchPosition.Value.Y;
        //     
        //     var gridRef = _viewModel.GetGridReference(x, y);
        //     var gridCrd = _viewModel.GetGridCoordinates(gridRef);
        //     Console.WriteLine($"Grid tapped at X: {x}, Y: {y} ==> {gridRef} ==> {gridCrd.x}, {gridCrd.y}");
        //
        //     var crosshairs = new CrosshairsOverlay();
        //     SelectedImageGrid.SetLayoutBounds(crosshairs, new Rect(gridCrd.x - (int)(gridCrd.width/2), gridCrd.y -(int)(gridCrd.height/2) , gridCrd.width, gridCrd.height)); // X, Y, Width, Height
        //     SelectedImageGrid.SetLayoutFlags(crosshairs, AbsoluteLayoutFlags.None);
        //     crosshairs.IsVisible = true;
        //     crosshairs.Text = gridRef;
        //     SelectedImageGrid.Children.Add(crosshairs);
        // }
    }

    
    // Need this to calculate the size of the image as rendered on the screen. 
    private async void SelectedImageOnSizeChanged(object? sender, EventArgs e) {
        if (sender is Image image) {
            _viewModel.ImageWidth = (int)image.Width;
            _viewModel.ImageHeight = (int)image.Height;
        }
    }

    private async void FileSystemButton_OnClicked(object? sender, EventArgs e) {
        Application.Current?.Dispatcher.Dispatch(async () => {
            var status = await CheckAndRequestFilePermissionsAsync();
            if (status == PermissionStatus.Granted || status == PermissionStatus.Limited) {
                var result = await FilePicker.PickAsync(new PickOptions {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select an image"
                });
                await _viewModel.SetImage(result);
            }
        });
    }

    private async void PhotosButton_OnClicked(object? sender, EventArgs e) {
        Application.Current?.Dispatcher.Dispatch(async () => {
            PermissionStatus status = await CheckAndRequestPhotosPermission();
            if (status == PermissionStatus.Granted || status == PermissionStatus.Limited) {
                var result = await MediaPicker.PickPhotoAsync();
                await _viewModel.SetImage(result);
            }
        });
    }

    public async Task<PermissionStatus> CheckAndRequestPhotosPermission() {
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status == PermissionStatus.Granted) {
            return status;
        }

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS) {
            return status;
        }

        if (Permissions.ShouldShowRationale<Permissions.Photos>()) { }
        status = await Permissions.RequestAsync<Permissions.Photos>();
        return status;
    }
    
    private async Task<PermissionStatus> CheckAndRequestFilePermissionsAsync() {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted){
            status = await Permissions.RequestAsync<Permissions.StorageRead>();
        }
        if (status != PermissionStatus.Granted) {
            await DisplayAlert("Permissions Denied", "Unable to access storage", "OK");
        }
        return status;
    }

    private async Task<bool> CheckAndRequestPhotoPermissionsAsync() {
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted) {
            status = await Permissions.RequestAsync<Permissions.Photos>();
        }

        if (status != PermissionStatus.Granted) {
            await DisplayAlert("Permissions Denied", "Unable to access photos", "OK");
            return false;
        }

        return true;
    }
}

