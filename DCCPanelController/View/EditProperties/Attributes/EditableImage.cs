using System.Diagnostics;
using DCCPanelController.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AttributesImageAttribute : Base.Attributes, IEditableAttribute {

    public IView? CreateView(EditableDetails value) {
        try {
            var stack = new StackLayout {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var imageString = PropertyHelper.GetPropertyValue<string>(value.Owner, value.Info.Name) ?? string.Empty;
            var image = new Image {
                Source = string.IsNullOrEmpty(imageString) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageString))) ?? null,
                WidthRequest = 200,
                HeightRequest = 200
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

            fileButton.Clicked += async (_, _) => await SelectImageAsync(image, value);

            var photoButton = new Button {
                Text = "Select Photo",
                BackgroundColor = StyleColor.Get("Primary"),
                TextColor = Colors.White,
                FontSize = 15,
                Margin = new Thickness(10, 10, 10, 10)
            };

            photoButton.Clicked += async (_, _) => await SelectPhotoAsync(image, value);
            horizontal.Children.Add(fileButton);
            horizontal.Children.Add(photoButton);
            stack.Children.Add(image);
            stack.Children.Add(horizontal);
            return stack;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create an Image: {e.Message}");
            return null;
        }
    }

    private async Task SelectPhotoAsync(Image image, EditableDetails value) {
        try {
            // Open file-picker to select an image
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
            PropertyHelper.SetPropertyValue<string>(value.Owner, value.Info.Name, base64String);
        } catch (Exception ex) {
            // Handle any errors
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
        }
    }

    private async Task SelectImageAsync(Image image, EditableDetails value) {
        try {
            // Open file-picker to select an image
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
            PropertyHelper.SetPropertyValue<string>(value.Owner, value.Info.Name, base64String);
        } catch (Exception ex) {
            // Handle any errors
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
        }
    }
}