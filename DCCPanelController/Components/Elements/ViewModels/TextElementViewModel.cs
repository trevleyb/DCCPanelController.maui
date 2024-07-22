using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Symbols;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements.ViewModels;

public partial class TextElementViewModel : ElementViewModel, IElementViewModel {
    
    public TextElementViewModel(TextPanelElement element, SymbolDetails details) : base(element, details) { }

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
    
}
