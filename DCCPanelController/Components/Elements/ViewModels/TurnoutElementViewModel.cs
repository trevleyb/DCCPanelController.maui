using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class TurnoutElementViewModel : ElementViewModel, IElementViewModel {
    public TurnoutElementViewModel(TurnoutPanelElement element, SymbolDetails details) : base(element, details) {
        TurnoutImage = _symbolDetails.Image;
    }

    [ObservableProperty] private ImageSource _turnoutImage;
    [ObservableProperty] private TurnoutStateEnum _turnoutState = TurnoutStateEnum.Unknown;

    private ImageSource SetTurnoutImage() {
        return TurnoutState switch {
            TurnoutStateEnum.Closed => _symbolDetails.Closed,
            TurnoutStateEnum.Thrown => _symbolDetails.Thrown,
            _                       => _symbolDetails.Image
        };
    }

    
    [RelayCommand]
    public async Task ToggleTurnout() {
        TurnoutState = TurnoutState == TurnoutStateEnum.Closed ? TurnoutStateEnum.Thrown : TurnoutStateEnum.Closed;
        TurnoutImage = SetTurnoutImage();
    }
}

public enum TurnoutStateEnum {
    Unknown, 
    Closed,
    Thrown
}