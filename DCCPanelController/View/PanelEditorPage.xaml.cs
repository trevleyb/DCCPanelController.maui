using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {

    public PanelEditorPage(Panel panel) {
        BindingContext = new PanelEditorViewModel(panel);
        InitializeComponent();
    }
}