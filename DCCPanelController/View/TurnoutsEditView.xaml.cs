using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : ContentView {
    private readonly ILogger<TurnoutsEditView> _logger;
    private readonly TurnoutsEditViewModel _viewModel;

    public TurnoutsEditView(ILogger<TurnoutsEditView> logger, TurnoutsEditViewModel viewModel) {
        InitializeComponent();
        _logger = logger;
        _viewModel = viewModel;
        _viewModel.Turnout.PropertyChanged += ViewModelOnPropertyChanged;
        BindingContext = _viewModel;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(TurnoutsEditViewModel.Turnout.Name) }) {
            _viewModel.Title = string.IsNullOrEmpty(_viewModel.Turnout.Name) ? "Turnout Properties" : _viewModel.Turnout.Name;
        }
    }
}