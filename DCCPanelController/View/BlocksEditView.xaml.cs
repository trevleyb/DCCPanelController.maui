using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class BlocksEditView : ContentView {
    private readonly ILogger<BlocksEditView> _logger;
    private readonly BlocksEditViewModel     _viewModel;

    public BlocksEditView(ILogger<BlocksEditView> logger, BlocksEditViewModel viewModel) {
        InitializeComponent();
        _logger = logger;
        _viewModel = viewModel;
        _viewModel.Block.PropertyChanged += ViewModelOnPropertyChanged;
        BindingContext = _viewModel;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(BlocksEditViewModel.Block.Name) }) {
            _viewModel.Title = string.IsNullOrEmpty(_viewModel.Block.Name) ? "Block Properties" : _viewModel.Block.Name;
        }
    }
}