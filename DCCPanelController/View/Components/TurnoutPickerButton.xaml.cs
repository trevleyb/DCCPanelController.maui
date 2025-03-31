using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.Components;

public partial class TurnoutPickerButton : ContentView {
    public static readonly BindableProperty SelectedTurnoutProperty = BindableProperty.Create(nameof(SelectedTurnout), typeof(Turnout), typeof(TurnoutPickerButton));
    public static readonly BindableProperty AvailableTurnoutsProperty = BindableProperty.Create(nameof(AvailableTurnouts), typeof(List<Turnout>), typeof(TurnoutPickerButton));

    public TurnoutPickerButton() {
        InitializeComponent();
        BindingContext = this;
    }

    public Turnout SelectedTurnout {
        get => (Turnout)GetValue(SelectedTurnoutProperty);
        set {
            SetValue(SelectedTurnoutProperty, value);
            OnPropertyChanged(nameof(SelectedTurnoutProperty)); // Update DisplayText when the color changes
        }
    }

    public List<Turnout> AvailableTurnouts {
        get => (List<Turnout>)GetValue(AvailableTurnoutsProperty);
        set {
            SetValue(AvailableTurnoutsProperty, value);
            OnPropertyChanged(nameof(AvailableTurnoutsProperty)); // Update DisplayText when the color changes
        }
    }

    private List<string> SelectableTurnouts => AvailableTurnouts.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name ?? "").ToList();

    [RelayCommand]
    private async Task ShowDropdownAsync() {
        var selectedItem = SelectedTurnout.Name ?? "";
        var popup = new IDPicker("Turnout", selectedItem, SelectableTurnouts);
        if (App.Current?.Windows[0]?.Page is Page { } mainpage) {
            var result = await mainpage.ShowPopupAsync(popup);
            if (result is string resultItem) {
                var found = AvailableTurnouts.Find(x => x.Name == resultItem);
                if (found is not null) SelectedTurnout = found;
            }
        }
    }
}