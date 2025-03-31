using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.Components;

public partial class RoutePickerButton : ContentView {
    public static readonly BindableProperty SelectedRouteProperty = BindableProperty.Create(nameof(SelectedRoute), typeof(Route), typeof(RoutePickerButton));
    public static readonly BindableProperty AvailableRoutesProperty = BindableProperty.Create(nameof(AvailableRoutes), typeof(List<Route>), typeof(RoutePickerButton));

    public RoutePickerButton() {
        InitializeComponent();
        BindingContext = this;
    }

    public Route SelectedRoute {
        get => (Route)GetValue(SelectedRouteProperty);
        set {
            SetValue(SelectedRouteProperty, value);
            OnPropertyChanged(nameof(SelectedRouteProperty)); // Update DisplayText when the color changes
        }
    }

    public List<Route> AvailableRoutes {
        get => (List<Route>)GetValue(AvailableRoutesProperty);
        set {
            SetValue(AvailableRoutesProperty, value);
            OnPropertyChanged(nameof(AvailableRoutesProperty)); // Update DisplayText when the color changes
        }
    }

    private List<string> SelectableRoutes => AvailableRoutes.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name ?? "").ToList();

    [RelayCommand]
    private async Task ShowDropdownAsync() {
        var selectedItem = SelectedRoute.Name ?? "";
        var popup = new IDPicker("Route", selectedItem, SelectableRoutes);
        if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
            var result = await mainpage.ShowPopupAsync(popup);
            if (result is string resultItem) {
                var found = AvailableRoutes.Find(x => x.Name == resultItem);
                if (found is not null) SelectedRoute = found;
            }
        }
    }
}