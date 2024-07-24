using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Model.Elements;
using NUnit.Framework;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class ButtonElementViewModel : ElementViewModel, IElementViewModel {
    public ButtonElementViewModel(ButtonPanelElement element, SymbolDetails details) : base(element, details) {
        ButtonStateImage = details.Image;
        _imageSourcebase = details.Image;
    }

    private readonly string _imageSourcebase;
    [ObservableProperty] private ImageSource _buttonStateImage;
    [ObservableProperty] private ButtonStateEnum _buttonState = ButtonStateEnum.Unknown;
    [ObservableProperty] private Color _color = Colors.Gainsboro;

    private Color SetButtonColor() {
        return ButtonState switch {
            ButtonStateEnum.Active   => Colors.Green,
            ButtonStateEnum.InActive => Colors.Red,
            _                        => Colors.Gainsboro
        };
    }

    private ImageSource SetButtonImage() {
        return ButtonState switch {
            ButtonStateEnum.Active   => _imageSourcebase.Replace(".png","_active.png"),
            ButtonStateEnum.InActive => _imageSourcebase.Replace(".png","_inactive.png"),
            _                        => _imageSourcebase.Replace(".png","_unknown.png"),
        };
    }

    
    [RelayCommand]
    public async Task ToggleButton() {
        ButtonState = ButtonState == ButtonStateEnum.Active ? ButtonStateEnum.InActive : ButtonStateEnum.Active;
        ButtonStateImage = SetButtonImage();
    }
}

public enum ButtonStateEnum {
    Unknown, 
    Active,
    InActive
}
