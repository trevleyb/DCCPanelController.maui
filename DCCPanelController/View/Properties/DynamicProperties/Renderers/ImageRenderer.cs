using DCCPanelController.Resources.Styles;
using DCCPanelController.Services;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ImageRenderer : BaseRenderer, IPropertyRenderer {
    private bool _isBusy;
    protected override int FieldWidth => 250;
    protected override int FieldHeight => -1;

    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Image;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        try {
            var spinner = new ActivityIndicator {
                IsRunning = false,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.Center,
            };
            var stack = new StackLayout {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 0),
            };
            var source = row.OriginalValue as string;
            var image = new Image {
                Source = string.IsNullOrEmpty(source) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(source))) ?? null,
                Aspect = Aspect.AspectFit,
                Margin = new Thickness(10, 10, 10, 10),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            var horizontal = new StackLayout {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 0),
            };

            // Add the File Button and process if it is clicked
            // -------------------------------------------------------------
            var fileButton = new Button {
                Text = "Select Image",
                BackgroundColor = StyleHelper.FromStyle("Primary"),
                TextColor = Colors.White,
                HeightRequest = 25,
                FontSize = 15,
                Margin = new Thickness(10, 0, 10, 0),
            };

            // Add the Photo Button and process if it is clicked
            // -------------------------------------------------------------
            var photoButton = new Button {
                Text = "Select Photo",
                BackgroundColor = StyleHelper.FromStyle("Primary"),
                TextColor = Colors.White,
                FontSize = 15,
                HeightRequest = 25,
                Margin = new Thickness(10, 0, 10, 0),
            };

            fileButton.Clicked += async (_, _) => {
                if (_isBusy) return;
                _isBusy = true;
                fileButton.IsEnabled = false;
                photoButton.IsEnabled = false;
                spinner.IsVisible = spinner.IsRunning = true;
                try {
                    var imageBase64 = await SelectImageAsyncOffUiThread();
                    if (!string.IsNullOrEmpty(imageBase64)) {
                        source = imageBase64;
                        image.Source = FromBase64(source);
                        SetValue(row, source);
                    }
                } finally {
                    spinner.IsRunning = spinner.IsVisible = false;
                    fileButton.IsEnabled = true;
                    photoButton.IsEnabled = true;
                    _isBusy = false;
                }
            };

            photoButton.Clicked += async (_, _) => {
                if (_isBusy) return;
                _isBusy = true;
                fileButton.IsEnabled = false;
                photoButton.IsEnabled = false;
                spinner.IsVisible = spinner.IsRunning = true;
                try {
                    var imageBase64 = await SelectPhotoAsyncOffUiThread();
                    if (!string.IsNullOrEmpty(imageBase64)) {
                        source = imageBase64;
                        image.Source = FromBase64(source);
                        SetValue(row, source);
                    }
                } finally {
                    spinner.IsRunning = spinner.IsVisible = false;
                    fileButton.IsEnabled = true;
                    photoButton.IsEnabled = true;
                    _isBusy = false;
                }
            };

            horizontal.Children.Add(fileButton);
            horizontal.Children.Add(photoButton);
            stack.Children.Add(image);
            stack.Children.Add(horizontal);
            stack.Children.Insert(0, spinner);
            return WrapWithLabel(ctx, stack);
        } catch {
            return new InvalidRenderer("Error generating Image.");
        }
    }

    private static ImageSource? FromBase64(string? b64) => string.IsNullOrEmpty(b64) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(b64)));

    private async Task<string> SelectImageAsyncOffUiThread() {
        try {
            var result = await MainThread.InvokeOnMainThreadAsync(() =>
                FilePicker.Default.PickAsync(new PickOptions {
                    PickerTitle = "Please select an image",
                    FileTypes = FilePickerFileType.Images,
                })
            );

            if (result == null) {
                await DisplayAlertHelper.DisplayOkAlertAsync("Cancelled", "No file was selected.");
                return"";
            }

            return await Task.Run(async () => {
                await using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return Convert.ToBase64String(ms.ToArray());
            });
        } catch (Exception ex) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
            return"";
        }
    }

    private async Task<string> SelectPhotoAsyncOffUiThread() {
        try {
            var result = await MainThread.InvokeOnMainThreadAsync(() => 
                MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "Please select a photo", CompressionQuality = 50, SelectionLimit = 1})
            );

            if (result == null) {
                await DisplayAlertHelper.DisplayOkAlertAsync("Cancelled", "No photo was selected.");
                return"";
            }
            
            var photo = result.FirstOrDefault();
            if (photo is not null) {
                await using var stream = await photo.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return Convert.ToBase64String(ms.ToArray());
            }

            return "";

        } catch (Exception ex) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Error", $"Unable to load Photo: {ex.Message}");
            return"";
        }
    }
}