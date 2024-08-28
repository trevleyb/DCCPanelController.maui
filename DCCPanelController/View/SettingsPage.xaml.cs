using System.ComponentModel;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {

    private Grid? _lastGridSelected;
    SettingsViewModel? _viewModel;
    
    public SettingsPage() {
        InitializeComponent();
    }

    void OnLabelTapped(object sender, EventArgs args) {
        if (_lastGridSelected is not null) {
            foreach (var item in _lastGridSelected.Children) {
                if (item is Label oldLabel) {
                    oldLabel.TextColor = Colors.Black;
                }
            }
        }

        if (sender is Grid grid) {
            foreach (var item in grid.Children) {
                if (item is Label newLabel) {
                    newLabel.TextColor = Colors.Green;
                }
            }
            _lastGridSelected = grid;
        }
    }

    private void OnEntryFocused(object sender, FocusEventArgs e) {
        var entry = sender as Entry;
        if (entry != null) {
            entry.CursorPosition = 0;
            entry.SelectionLength = entry.Text?.Length ?? 0;
        }
    }

    private void About_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }
    private void Instructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }
}

public class EntryValidationBehavior : Behavior<Entry> {
    protected override void OnAttachedTo(Entry entry) {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry) {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs args) {
        if (sender is Entry entry) {
            if (!string.IsNullOrEmpty(args.NewTextValue)) {
                if (int.TryParse(args.NewTextValue, out var result)) {
                    entry.TextColor = (result is >= 0 and <= 255) ? Colors.Black : Colors.Red;
                }
            }
        }
    }
}
