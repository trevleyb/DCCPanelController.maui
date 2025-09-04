using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.View;

public struct SomeTestItem  {
    public required string Name { get; set; }
    public required string Id { get; set; }
}

public partial class TestPageViewModel : ObservableObject { }

