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

public partial class PropertyPage : ContentPage {
    public PropertyPage(ITrackPiece trackPiece) {
        InitializeComponent();
        var propertyName = trackPiece.GetType().Name ?? "Unknonwn";
        BindingContext = new PropertyPageViewModel(propertyName, trackPiece, TableViewContainer);
    }

    private void ClosePropertyPage(object? sender, EventArgs e) {
        Navigation.PopModalAsync();
    }
}