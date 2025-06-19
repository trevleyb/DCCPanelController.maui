using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Helpers;

public partial class PanelColorItem : ObservableObject {
    private readonly Func<Panel, Color> _getColorFunc;
    private readonly Panel _panel;
    private readonly Action<Panel, Color> _setColorAction;

    public PanelColorItem(Panel panel,
                          string category,
                          string labelText,
                          string panelPropertyName,
                          Func<Panel, Color> getColorFunc,
                          Action<Panel, Color> setColorAction,
                          bool allowsNoColor = true) {

        _panel = panel ?? throw new ArgumentNullException(nameof(panel));
        _getColorFunc = getColorFunc;
        _setColorAction = setColorAction;
                
        LabelText = labelText;
        AllowsNoColor = allowsNoColor;
        Category = category;

        if (_panel is INotifyPropertyChanged npcPanel) {
            npcPanel.PropertyChanged += (sender, args) => {
                if (args.PropertyName == panelPropertyName) {
                    OnPropertyChanged(nameof(ColorValue));
                }
            };
        }
    }

    public string LabelText { get; }
    public string Category { get; }
    public bool AllowsNoColor { get; }
    
    public Color ColorValue {
        get => _getColorFunc(_panel);
        set {
            if (!EqualityComparer<Color>.Default.Equals(_getColorFunc(_panel), value)) {
                _setColorAction(_panel, value); 
                OnPropertyChanged();            
            }
        }
    }
}