using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DCCPanelController.Helpers;

namespace ColorPickerControl
{
    public partial class ColorPopup : Popup
    {
        public ObservableCollection<ColorOption> ColorOptions { get; private set; }

        public ColorPopup()
        {
            InitializeComponent();

            // Retrieve all colors from the Colors class and add to the list
            var colors = PredefinedColors.GetColors(); 
            ColorOptions = new ObservableCollection<ColorOption>(colors);
            BindingContext = this;
        }

        private void OnColorSelected(object sender, SelectionChangedEventArgs e) {
            // Close the popup and pass the selected color back
            if (e.CurrentSelection.FirstOrDefault() is ColorOption selectedColor) {
                Close(selectedColor);
            }
        }
    }
}