using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCPanelController.View;

public partial class TestPage : ContentPage {
    public TestPage(TestPageViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;
    }
}