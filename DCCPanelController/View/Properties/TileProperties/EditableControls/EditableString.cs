using System.Reflection;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableString(string label, string description = "", int order = 0, string? group = null, int width = 300)
    : EditableProperty(label, description, order, group), IEditableProperty {
    
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var box = new Border() {
                Margin = new Thickness(0, 5, 5, 5),
                WidthRequest = width,
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Transparent,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush((Color?)Application.Current?.Resources["Primary"] ?? Colors.Black),
                StrokeShape = new RoundRectangle {
                    CornerRadius = new CornerRadius(10) // All corners rounded with radius 10
                }
            };
            
            var cell = new Entry {
                Margin = new Thickness(5, 5, 5, 5),
                Placeholder = Label,
                Keyboard = Keyboard.Text,
                WidthRequest = 500,
                HeightRequest = 25,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };
            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(Entry.Text)) SetModified(true);
                if (sender is Entry entry && Value is string) {
                    SetModified(!Value.Equals(entry.Text));
                }
            };
            box.Content = cell;
            return CreateGroupCell(box);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }
}