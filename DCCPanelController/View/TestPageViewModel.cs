using AVFoundation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View;

public struct SomeTestItem {
    public required string Name { get; set; }
    public required string Id { get; set; }
}

public partial class TestPageViewModel : ObservableObject {
    public TestPageViewModel() { }
}

