using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;

// Assuming Panel is here

namespace DCCPanelController.View.Properties.PanelProperties; // Or your preferred ViewModel namespace

public class ColorSettingItemViewModel : ObservableObject {
    private readonly Func<Panel, Color> _getColorFunc;
    private readonly Panel _panel;
    private readonly string _panelPropertyName; // e.g., nameof(Panel.BackgroundColor)
    private readonly Action<Panel, Color> _setColorAction;

    public ColorSettingItemViewModel(Panel panel,
                                     string labelText,
                                     string panelPropertyName,
                                     Func<Panel, Color> getColorFunc,
                                     Action<Panel, Color> setColorAction,
                                     bool allowsNoColor = true) {
        _panel = panel ?? throw new ArgumentNullException(nameof(panel));
        LabelText = labelText;
        _panelPropertyName = panelPropertyName;
        _getColorFunc = getColorFunc;
        _setColorAction = setColorAction;
        AllowsNoColor = allowsNoColor;

        if (_panel is INotifyPropertyChanged npcPanel) {
            npcPanel.PropertyChanged += (sender, args) => {
                if (args.PropertyName == _panelPropertyName) {
                    // The underlying panel property changed from somewhere else,
                    // so our ColorValue needs to notify the UI to re-read.
                    OnPropertyChanged(nameof(ColorValue));
                }
            };
        }
    }

    public string LabelText { get; }
    public bool AllowsNoColor { get; }

    public Color ColorValue {
        get => _getColorFunc(_panel);
        set {
            if (!EqualityComparer<Color>.Default.Equals(_getColorFunc(_panel), value)) {
                _setColorAction(_panel, value); // This should make Panel raise its own PropertyChanged
                OnPropertyChanged();            // Notify that ColorValue of this item changed
            }
        }
    }
}