using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class BlocksEditView : ContentView {
    private readonly ILogger<BlocksEditViewModel> _logger;
    private readonly BlocksEditViewModel     _viewModel;

    public BlocksEditView(ILogger<BlocksEditViewModel> logger, BlocksEditViewModel viewModel) {
        _logger = logger;
        _viewModel = viewModel;
        _viewModel.Block.PropertyChanged += ViewModelOnPropertyChanged;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(BlocksEditViewModel.Block.Name) }) {
            _viewModel.Title = string.IsNullOrEmpty(_viewModel.Block.Name) ? "Block Properties" : _viewModel.Block.Name;
        }
    }

    private void Picker_OnSelectedIndexChanged(object? sender, EventArgs e) {
        if (sender is Picker { SelectedItem: Sensor sensor }) _viewModel?.Block?.Sensor = sensor.SystemId;
    }
}