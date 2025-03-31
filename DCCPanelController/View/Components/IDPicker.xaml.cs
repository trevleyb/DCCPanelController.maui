using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Components;

public partial class IDPicker : Popup {
    private string _selectedItem;
    private readonly string? _itemType;
    
    public IDPicker(string itemType, string item, List<string> items) {
        _itemType = itemType;
        _selectedItem = item;
        Items = items;
        InitializeComponent();
        BindingContext = this;
    }

    public string Prompt => "Select a "+ _itemType;
    public List<string> Items { get; init; }
    
    public string Item {
        get => _selectedItem;
        set {
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private async Task CloseOnSelectedAsync() {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(Item, cts.Token);
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        OnPropertyChanged(nameof(Item));
    }
}