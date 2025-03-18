using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;

namespace DCCPanelController.View.DynamicProperties;

public class EditableImage : EditableProperty, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var stack = new StackLayout {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var imageString = PropertyHelper.GetPropertyValue<string>(owner, info.Name) ?? string.Empty;

            var image = new Image {
                Source = string.IsNullOrEmpty(imageString) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageString))) ?? null,
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                MaximumHeightRequest = 200,
                MaximumWidthRequest = 100
            };

            var horizontal = new StackLayout {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var fileButton = new Button {
                Text = "Select Image",
                BackgroundColor = StyleColor.Get("Primary"),
                TextColor = Colors.White,
                FontSize = 15,
                Margin = new Thickness(10, 10, 10, 10)
            };

            fileButton.Clicked += async (_, _) => await SelectImageAsync(image, owner, info);

            var photoButton = new Button {
                Text = "Select Photo",
                BackgroundColor = StyleColor.Get("Primary"),
                TextColor = Colors.White,
                FontSize = 15,
                Margin = new Thickness(10, 10, 10, 10)
            };

            photoButton.Clicked += async (_, _) => await SelectPhotoAsync(image, owner, info);
            horizontal.Children.Add(fileButton);
            horizontal.Children.Add(photoButton);
            stack.Children.Add(image);
            stack.Children.Add(horizontal);
            return CreateGroupCell(stack, owner, info, attribute);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create an Image: {e.Message}");
            return null;
        }
    }
    
    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new ViewCell() { View = CreateView(owner, info, attribute) as Microsoft.Maui.Controls.View };
    }

    private async Task SelectPhotoAsync(Image image, object owner, PropertyInfo info) {
        try {
            // Open the file-picker to select an image
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions {
                Title = "Please select an image"
            });

            if (result == null) {
                // Handle when the user cancels the file picker
                await DisplayAlertHelper.DisplayOkAlertAsync("Cancelled", "No file was selected.");
                return;
            }

            // Read the selected file's content as a byte array
            var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Convert to Base64 string
            var base64String = Convert.ToBase64String(fileBytes);
            image.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64String)));
            PropertyHelper.SetPropertyValue<string>(owner, info.Name, base64String);
        } catch (Exception ex) {
            // Handle any errors
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
        }
    }

    private async Task SelectImageAsync(Image image, object owner, PropertyInfo info) {
        try {
            // Open the file-picker to select an image
            var result = await FilePicker.Default.PickAsync(new PickOptions {
                PickerTitle = "Please select an image",
                FileTypes = FilePickerFileType.Images // Restrict to image files only
            });

            if (result == null) {
                // Handle when the user cancels the file picker
                await DisplayAlertHelper.DisplayOkAlertAsync("Cancelled", "No file was selected.");
                return;
            }

            // Read the selected file's content as a byte array
            var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Convert to Base64 string
            var base64String = Convert.ToBase64String(fileBytes);
            image.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64String)));
            PropertyHelper.SetPropertyValue<string>(owner, info.Name, base64String);
        } catch (Exception ex) {
            // Handle any errors
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
        }
    }
}