using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {

    private Grid? _lastGridSelected;
    
    public SettingsPage() {
        InitializeComponent();
        var service = new SettingsService();
        var viewModel = new SettingsViewModel(service);
        BindingContext = viewModel;
    }

    void OnLabelTapped(object sender, EventArgs args) {

        if (_lastGridSelected is not null) {
            foreach (var item in _lastGridSelected.Children) {
                if (item is Label oldLabel) {
                    oldLabel.TextColor = Colors.Black;
                }
            }
        }

        var grid = (Grid)sender;
        foreach (var item in grid.Children) {
            if (item is Label newLabel) {
                newLabel.TextColor = Colors.Green;
            }
        }
        _lastGridSelected = grid;
    }

    public void OnEntryFocused(object sender, FocusEventArgs e)
    {
        var entry = sender as Entry;
        if (entry != null) {
            entry.CursorPosition = 0;
            entry.SelectionLength = entry.Text?.Length ?? 0;
        }
    }

}

public class EntryValidationBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs args) {
        bool isValid = false;
        if (!string.IsNullOrEmpty(args.NewTextValue)) {
            if (int.TryParse(args.NewTextValue, out int result)) {
                isValid = result is >= 0 and <= 255;
            }
        }
        ((Entry)sender).TextColor = isValid ? Colors.Black : Colors.Red;
    }
}