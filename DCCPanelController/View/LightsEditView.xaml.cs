using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class LightsEditView : ContentView {
    private readonly ILogger<LightsEditViewModel> _logger;
    private readonly LightsEditViewModel     _viewModel;

    public LightsEditView(ILogger<LightsEditViewModel> logger, LightsEditViewModel viewModel) {
        _logger = logger;
        _viewModel = viewModel;
        _viewModel.Light.PropertyChanged += ViewModelOnPropertyChanged;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(LightsEditViewModel.Light.Name) }) {
            _viewModel.Title = string.IsNullOrEmpty(_viewModel.Light.Name) ? "Light Properties" : _viewModel.Light.Name;
        }
        if (e is { PropertyName: nameof(LightsEditViewModel.Light.SystemId) }) {
            _viewModel.Light.InferDccAddressFrom(_viewModel?.Light?.SystemId ?? "");
        }
    }
}