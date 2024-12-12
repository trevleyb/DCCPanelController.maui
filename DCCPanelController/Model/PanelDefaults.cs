using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class PanelDefaults : ObservableObject {
    [ObservableProperty] private Color _backgroundColor    = Colors.White;
    [ObservableProperty] private Color _mainLineColor      = Colors.Green;
    [ObservableProperty] private Color _branchLineColor    = Colors.Grey;
    [ObservableProperty] private Color _divergingColor     = Colors.Grey;
    [ObservableProperty] private Color _buttonOnColor      = Colors.Lime;
    [ObservableProperty] private Color _buttonOffColor     = Colors.Crimson;
    [ObservableProperty] private Color _buttonUnknownColor = Colors.LightGray;
    [ObservableProperty] private Color _occupiedColor      = Colors.Crimson;
    [ObservableProperty] private Color _hiddenColor        = Colors.White;
    [ObservableProperty] private Color _terminatorColor    = Colors.Black;
    [ObservableProperty] private Color _buttonBorder       = Colors.Crimson;
    [ObservableProperty] private Color _continuationColor  = Colors.Black;
    [ObservableProperty] private Color _borderColor        = Colors.Black;

    public void ResetToDefaults() {
        BackgroundColor    = Colors.White;
        MainLineColor      = Colors.Green;
        BranchLineColor    = Colors.Grey;
        DivergingColor     = Colors.Grey;
        ButtonOnColor      = Colors.Lime;
        ButtonOffColor     = Colors.Crimson;
        ButtonUnknownColor = Colors.LightGray;
        OccupiedColor      = Colors.Crimson;
        HiddenColor        = Colors.White;
        TerminatorColor    = Colors.Black;
        ButtonBorder       = Colors.Crimson;
        ContinuationColor  = Colors.Black;
        BorderColor        = Colors.Black;
    }
    
    public PanelDefaults Clone() {
        return ObjectCloner.Clone<PanelDefaults>(this) ?? throw new ArgumentException("Cannot clone the Panel Defaults.");
    }
}
