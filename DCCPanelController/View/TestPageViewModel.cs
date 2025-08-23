using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View;

public struct SomeTestItem() {
    public required string Name { get; set; }
    public required string Id { get; set; }
}

public partial class TestPageViewModel : ObservableObject {

    [ObservableProperty] private bool _canSetModes;
    [ObservableProperty] private string _selectedValue;
    [ObservableProperty] private SomeTestItem _selectedItem;
    [ObservableProperty] private List<SomeTestItem> _testItems;
    
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Panel _panel;
    
    [RelayCommand]
    public async Task SetMode() {
        CanSetModes = !CanSetModes;
    }
    
    public TestPageViewModel() {
        _testItems = new List<SomeTestItem>();
        _testItems.Add(new SomeTestItem {Name = "Test 1", Id = "1"});
        _testItems.Add(new SomeTestItem {Name = "Test 2", Id = "2"});
        _testItems.Add(new SomeTestItem {Name = "Test 3", Id = "3"});
        _testItems.Add(new SomeTestItem {Name = "Test 4", Id = "4"});
        _testItems.Add(new SomeTestItem {Name = "Test 5", Id = "5"});
        _selectedItem = _testItems[2];
        _selectedValue = _selectedItem.Id;
        
        Panels = [];
        Panel = Panels.CreatePanel();
    }
    
}