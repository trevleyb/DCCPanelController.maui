using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class ControlPanelPage : ContentPage, INotifyPropertyChanged {
    public ControlPanelPage() {
        InitializeComponent();
    }

    public void UpdateGrid() {
        if (BindingContext is ControlPanelViewModel { } viewModel) {
            
            ControlPanelGrid.Children.Clear();
            ControlPanelGrid.RowDefinitions.Clear();
            ControlPanelGrid.ColumnDefinitions.Clear();
            
        }
    }
}