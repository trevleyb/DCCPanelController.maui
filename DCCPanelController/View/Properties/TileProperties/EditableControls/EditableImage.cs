using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Services;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableImage(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
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
                Margin = new Thickness(10, 10, 10, 10),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
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
                HeightRequest = 30,
                FontSize = 15,
                Margin = new Thickness(10, 10, 10, 10)
            };
            fileButton.Clicked += async (_, _) => await SelectImageAsync(image, owner, info);

            var photoButton = new Button {
                Text = "Select Photo",
                BackgroundColor = StyleColor.Get("Primary"),
                TextColor = Colors.White,
                FontSize = 15,
                HeightRequest = 30,
                Margin = new Thickness(10, 10, 10, 10)
            };
            photoButton.Clicked += async (_, _) => await SelectPhotoAsync(image, owner, info);
            horizontal.Children.Add(fileButton);
            horizontal.Children.Add(photoButton);
            stack.Children.Add(image);
            stack.Children.Add(horizontal);
            return CreateGroupCell(stack, 150);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create an Image: {Message}",e.Message);
            return null;
        }
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
            SetModified(true);
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
            SetModified(true);
        } catch (Exception ex) {
            // Handle any errors
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
        }
    }
}