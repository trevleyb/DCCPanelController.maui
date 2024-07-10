using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.View;
using ImageDetection;
using SixLabors.ImageSharp.PixelFormats;

namespace DCCPanelController.ViewModel;

public partial class PanelsDetailsViewModel : BaseViewModel {

    private const int Rows = 18;
    private const int Cols = 24;

    [ObservableProperty] private Panel _panel;
    [ObservableProperty] private ObservableCollection<TurnoutPoint> _turnoutPoints;
    [ObservableProperty] private int _imageWidth;
    [ObservableProperty] private int _imageHeight;

    [NotifyPropertyChangedFor(nameof(HasNoImage))] [ObservableProperty]
    private bool _hasImage;

    public bool HasNoImage => !HasImage;

    private int _draggingIndex;
    private readonly Page? _page;
    private Panel? _original;

    // This breaks the view model so we need to solve this later 
    public PanelsDetailsViewModel(Panel panel, Page page) {
        Panel = panel;
        _original = Panel.Duplicate;
        _page = page;
        TurnoutPoints = Panel.Turnouts;
        HasImage = Panel.Image != null;
        GetTurnoutPointsCommand.Execute(null);
    }

    [RelayCommand]
    public async Task SetImage(FileResult? result) {
        if (result != null) {
            Panel.ImageAsBase64 = await Panel.ConvertToBase64Async(result);
            HasImage = true;
            OnPropertyChanged(nameof(Panel.Image));
        }
    }

    [RelayCommand]
    public async Task ClearImageAsync() {
        Panel.ImageAsBase64 = null;
        HasImage = false;
        OnPropertyChanged(nameof(Image));
        OnPropertyChanged(nameof(Panel));
    }

    [RelayCommand]
    void Validate() {
        ValidateAllProperties();
    }

    [RelayCommand]
    public async Task EditTurnoutPointsAsync(TurnoutPoint point) { }

    [RelayCommand]
    public async Task DeleteTurnoutAsync(TurnoutPoint point) {
        Panel.Turnouts.Remove(point);
        HasImage = Panel.Image != null;
    }

    [RelayCommand]
    public async Task AddNewTurnoutPoint() {
        if (IsBusy) return;
        try {
            IsBusy = true;
            var newTurnoutPoint = new TurnoutPoint();
            Panel.Turnouts.Add(newTurnoutPoint);
            EditTurnoutPointsCommand.Execute(newTurnoutPoint);
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to get Turnout States: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Turnout States", ex.Message, "OK");
        } finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task RestorePanelAsync() {
        if (IsBusy) return;
        if (_page != null && _original != null) {
            Panel = _original.Duplicate;
            await _page.Navigation.PopAsync();
        }
    }


    [RelayCommand]
    public async Task SavePanelAsync() {
        if (IsBusy) return;
        if (_page != null) {
            IsBusy = true;
            var service = App.ServiceProvider?.GetService<SettingsService>();
            service?.Save();
            IsBusy = false;
            await _page.Navigation.PopAsync();
        }
    }

    [RelayCommand]
    public async Task DragAsync(TurnoutPoint point) {
        _draggingIndex = TurnoutPoints.IndexOf(point);
    }

    [RelayCommand]
    public async Task DropAsync(TurnoutPoint point) {
        var droppedIndex = TurnoutPoints.IndexOf(point);

        // Swap or rearrange items
        if (_draggingIndex >= 0 && droppedIndex >= 0) {
            var draggedItem = TurnoutPoints[_draggingIndex];
            TurnoutPoints.Remove(draggedItem);
            TurnoutPoints.Insert(droppedIndex, draggedItem);

            // ReApply the Sort Order so we order the list by this number
            // ------------------------------------------------------------
            for (int index = 0; index < TurnoutPoints.Count; index++) {
                TurnoutPoints[index].SortOrder = index;
            }
        }
    }

    [RelayCommand]
    public async Task GetTurnoutPointsAsync() {
        if (IsBusy) return;
        try {
            IsBusy = true;
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to get Turnout States: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Turnout States", ex.Message, "OK");
        } finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

}
    
    

