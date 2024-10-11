using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DCCPanelController.Helpers;

namespace ColorPickerControl
{
    public partial class ColorDropdown : ContentView
    {
        // Command to show the color dropdown popup
        public ICommand ShowDropdownCommand { get; set; }

        public ColorDropdown()
        {
            InitializeComponent();

            PlaceholderText = "Click here to select a color";
            ShowDropdownCommand = new Command(ShowDropdown);
            BindingContext = this;
        }

        // Asynchronously show the popup and update the selected color
        private async void ShowDropdown()
        {
            var popup = new ColorPopup();
            var result = await Application.Current.MainPage.ShowPopupAsync(popup);

            if (result is ColorOption selectedColor) {
                SelectedColor = selectedColor.Color;
                SelectedColorName = selectedColor.Name;
            }
        }

        // Property for the currently selected color
        public Color SelectedColor {
            get => (Color)GetValue(SelectedColorProperty);
            set {
                SetValue(SelectedColorProperty, value);
                OnPropertyChanged(nameof(DisplayText)); // Update DisplayText when the color changes
                OnPropertyChanged(nameof(TextContrastColor)); // Update DisplayText when the color changes
            }
        }

        // Bindable property for SelectedColor
        public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
            nameof(SelectedColor), typeof(Color), typeof(ColorDropdown), Colors.White);

        // Property for the name of the currently selected color
        public string SelectedColorName {
            get => (string)GetValue(SelectedColorNameProperty);
            set {
                SetValue(SelectedColorNameProperty, value);
                OnPropertyChanged(nameof(DisplayText)); 
                OnPropertyChanged(nameof(TextContrastColor)); // Update DisplayText when the color changes
            }
        }

        // Bindable property for SelectedColorName
        public static readonly BindableProperty SelectedColorNameProperty = BindableProperty.Create(
            nameof(SelectedColorName), typeof(string), typeof(ColorDropdown), string.Empty);

        // Property for the placeholder text when no color is selected
        public string PlaceholderText {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        // Bindable property for PlaceholderText
        public static readonly BindableProperty PlaceholderTextProperty = BindableProperty.Create(
            nameof(PlaceholderText), typeof(string), typeof(ColorDropdown), string.Empty);

        // Property to determine what text should be displayed on the button
        public string DisplayText => string.IsNullOrWhiteSpace(SelectedColorName) ? PlaceholderText : SelectedColorName;
        public Color TextContrastColor => GetContrastingColor(SelectedColor);        
        
        public static bool IsColorDark(Color color) {
            // Using relative luminance to determine if the color is dark or light
            double brightness = ((color.Red * 255 * 299) +
                                 (color.Green * 255 * 587) +
                                 (color.Blue * 255 * 114)) / 1000;
            return brightness < 128;
        }

        public static Color GetContrastingColor(Color backgroundColor) {
            return IsColorDark(backgroundColor) ? Colors.White : Colors.Black;
        }
        
        
    }
    
}
