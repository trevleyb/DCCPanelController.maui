using DCCPanelController.Components.TrackComponents;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class TextElementViewModel : ElementViewModel, IElementViewModel {
    
    public TextPanelElement TextElement => (TextPanelElement)Element; 
    public TextElementViewModel(TextPanelElement element, SymbolDetails details) : base(element, details) { }

    /*
    public string Text {
        get => ((TextPanelElement)Element).Text;
        set {
            ((TextPanelElement)Element).Text = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    public int FontSize {
        get => ((TextPanelElement)Element).FontSize;
        set {
            ((TextPanelElement)Element).FontSize = value;
            OnPropertyChanged(nameof(FontSize));
        }
    }

    public string Font {
        get => ((TextPanelElement)Element).Font;
        set {
            ((TextPanelElement)Element).Font = value;
            OnPropertyChanged(nameof(Font));
        }
    }
    
    public Color FontColor {
        get => ((TextPanelElement)Element).FontColor;
        set {
            ((TextPanelElement)Element).FontColor = value;
            OnPropertyChanged(nameof(FontColor));
        }
    }
    public Color BackgroundColor {
        get => ((TextPanelElement)Element).BackgroundColor;
        set {
            ((TextPanelElement)Element).BackgroundColor = value;
            OnPropertyChanged(nameof(BackgroundColor));
        }
    }
    */
}
