using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class SensorsEditView : ContentView {
    private readonly ILogger<SensorsEditViewModel> _logger;
    private readonly SensorsEditViewModel     _viewModel;

    public SensorsEditView(ILogger<SensorsEditViewModel> logger, SensorsEditViewModel viewModel) {
        InitializeComponent();
        _logger = logger;
        _viewModel = viewModel;
        _viewModel.Sensor.PropertyChanged += ViewModelOnPropertyChanged;
        BindingContext = _viewModel;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(SensorsEditViewModel.Sensor.Name) }) {
            _viewModel.Title = string.IsNullOrEmpty(_viewModel.Sensor.Name) ? "Sensor Properties" : _viewModel.Sensor.Name;
        }
        if (e is { PropertyName: nameof(SensorsEditViewModel.Sensor.Id) }) {
            _viewModel.Sensor.SetDccAddress(_viewModel.Sensor.Id);
        }
    }
}