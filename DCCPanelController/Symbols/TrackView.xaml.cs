using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Symbols.TrackViewModels;

namespace DCCPanelController.Symbols.Tracks;

public partial class TrackView : ContentView {

    public ITrackViewModel ViewModel { get; init; }
    
    public TrackView(ITrackViewModel viewModel) {
        ViewModel = viewModel;
        InitializeComponent();
        BindingContext = ViewModel;
    }
}