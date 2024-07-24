using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class CircleTextElementViewModel : ElementViewModel, IElementViewModel {

    public CircleTextElementViewModel(CircleTextPanelElement element, SymbolDetails details) : base(element, details) { }
    
    public string Text {
        get => ((CircleTextPanelElement)Element).Text;
        set {
            ((CircleTextPanelElement)Element).Text = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    public int FontSize {
        get => ((CircleTextPanelElement)Element).FontSize;
        set {
            ((CircleTextPanelElement)Element).FontSize = value;
            OnPropertyChanged(nameof(FontSize));
        }
    }

    public string Font {
        get => ((CircleTextPanelElement)Element).Font;
        set {
            ((CircleTextPanelElement)Element).Font = value;
            OnPropertyChanged(nameof(Font));
        }
    }
    
    public Color FontColor {
        get => ((CircleTextPanelElement)Element).FontColor;
        set {
            ((CircleTextPanelElement)Element).FontColor = value;
            OnPropertyChanged(nameof(FontColor));
        }
    }
}
