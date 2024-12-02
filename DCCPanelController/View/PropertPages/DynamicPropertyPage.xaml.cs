using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Tracks.Base;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class DynamicPropertyPage : ContentPage {
    public DynamicPropertyPage(ITrackPiece obj, string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel(obj, propertyName, PropertyContainer);
    }

    private void ClosePropertyPage(object? sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}