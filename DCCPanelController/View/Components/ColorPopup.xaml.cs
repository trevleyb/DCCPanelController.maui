using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DCCPanelController.Helpers;

namespace ColorPickerControl {
    public partial class ColorPopup : Popup {
        public ObservableCollection<ColorOption> ColorOptions { get; private set; }

        public ColorPopup() {
            InitializeComponent();
            var colors = PredefinedColors.GetColors();
            ColorOptions = new ObservableCollection<ColorOption>(colors);
            BindingContext = this;
        }

        private void OnColorSelected(object sender, SelectionChangedEventArgs e) {
            var selected = (e.CurrentSelection.Count > 0 ? e.CurrentSelection[0] : null) as ColorOption;
            Close(selected);
        }
    }
}