using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class PanelsDetailsViewModel : BaseViewModel {

    public Panel Panel { get; init; }
    public ObservableCollection<TurnoutPoint> TurnoutPoints => Panel.Turnouts;

    private int _draggingIndex;
    private readonly Panel? _original;
    private readonly Page? _page;
    
    // This breaks the view model so we need to solve this later 
    public PanelsDetailsViewModel(Panel panel, Page page) {
        Panel = panel;
        _original = (Panel?)panel.Clone();
        _page = page;
        GetTurnoutPointsCommand.Execute(null);
    }

    [RelayCommand]
    public async Task EditTurnoutPointsAsync(TurnoutPoint point) { }

    [RelayCommand]
    public async Task DeleteTurnoutAsync(TurnoutPoint point) {
        Panel.Turnouts.Remove(point);
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
    public async Task SavePanelAsync() {
        if (_page != null) {
            var service = App.ServiceProvider?.GetService<SettingsService>();
            service?.Save();
            await _page.Navigation.PopAsync();
        }
    }

    [RelayCommand]
    public async Task CancelPanelAsync() {
        if (_page != null) {
            _original?.Copy(Panel);
            await _page.Navigation.PopAsync();
        }
    }
    
    [RelayCommand]
    public async Task DragAsync(TurnoutPoint point) { 
        _draggingIndex = TurnoutPoints.IndexOf(point);
    }

    [RelayCommand]
    public async Task DropAsync(TurnoutPoint point) {
        int droppedIndex = TurnoutPoints.IndexOf(point);

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
        }
        catch (Exception ex) {
            Debug.WriteLine($"Unable to get Turnout States: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Turnout States", ex.Message, "OK");
        }
        finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}

