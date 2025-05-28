using System.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class TurnoutsEditView : ContentView {
    private readonly TurnoutsEditViewModel _viewModel;
    public TurnoutsEditView(TurnoutsEditViewModel viewModel) {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.Turnout.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e is { PropertyName: nameof(TurnoutsEditViewModel.Turnout.Name)}) {
            _viewModel.Title = string.IsNullOrEmpty(_viewModel.Turnout.Name) ? "Turnout Properties" : _viewModel.Turnout.Name;
        }
    }
}