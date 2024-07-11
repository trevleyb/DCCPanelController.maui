using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.ViewModel;

namespace DCCPanelController.Symbols;

public partial class DropZone : ContentView {
    public DropZone(PanelEditorViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }
}