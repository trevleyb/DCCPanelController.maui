using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.View.PropertPages;

public partial class TextPropertyPage : ContentView {
    public TextPropertyPage(TextPanelElement element) {
        InitializeComponent();
        BindingContext = element;
    }
}