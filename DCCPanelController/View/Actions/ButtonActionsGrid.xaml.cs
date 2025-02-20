using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    public ButtonActionsGrid(ButtonActions buttonActions, ITrackPiece trackPiece) {
        InitializeComponent();
        BindingContext = new ButtonActionsGridViewModel(buttonActions, trackPiece);
    }
}