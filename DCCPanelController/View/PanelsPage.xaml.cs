using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {

    public PanelsPage() {
        InitializeComponent();
        var viewModel = App.ServiceProvider?.GetService<PanelsViewModel>();
        if (viewModel != null) {
            viewModel.Sender = this;
            BindingContext = viewModel;
        }
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        if (BindingContext is PanelsViewModel viewModel) {
            viewModel.Save();
            
        }
    }
}