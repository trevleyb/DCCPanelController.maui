using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using Microsoft.Maui.Graphics.Platform;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableValidator, ICloneable {

    [ObservableProperty]
    [Required][NoSpaces]
    private string _id = "new";
    
    [ObservableProperty] 
    private string _name = string.Empty;
    
    [ObservableProperty] 
    private int _sortOrder = 0;
    
    [ObservableProperty] 
    private ObservableCollection<TurnoutPoint> _turnouts = [];

    [ObservableProperty]
    [JsonIgnore]
    private int _cardHeight;

    [ObservableProperty] private int _originalImageWidth;
    [ObservableProperty] private int _originalImageHeight;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(Image))]
    [NotifyPropertyChangedFor(nameof(IsImageAvailable))]
    private string? _imageAsBase64;
    
    [JsonIgnore]
    public bool IsImageAvailable => Image != null;
    
    public ImageSource? Image {
        get {
            if (string.IsNullOrEmpty(ImageAsBase64)) return null;
            try {
                var imageBytes = Convert.FromBase64String(ImageAsBase64);
                var image = PlatformImage.FromStream(new MemoryStream(imageBytes));
                OriginalImageHeight = (int)image.Height;
                OriginalImageWidth = (int)image.Width;
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
    
    public static async Task<string?> ConvertToBase64Async(FileResult? fileResult) {
        if (fileResult == null) return null;
        using (var stream = await fileResult.OpenReadAsync()) {
            using (var memoryStream = new MemoryStream()) {
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
    }

    public void CopyTo(Panel target) {
        CopyTo(this, target);
    }

    public Panel CopyTo(Panel source, Panel target) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        target.Id = source.Id;
        target.Name = source.Name;
        target.SortOrder = source.SortOrder;
        target.ImageAsBase64 = source.ImageAsBase64;
        target.Turnouts = [];
        foreach (var tp in source.Turnouts) {
            target.Turnouts.Add(new TurnoutPoint { Id = tp.Id, Name = tp.Name, State = tp.State });
        }
        return target;
    }

    [JsonIgnore]
    public Panel Duplicate => (Panel)Clone();
    
    public object Clone() {
        return CopyTo(this,new Panel());
    }
}

[JsonSerializable(typeof(List<Panel>))]
internal sealed partial class PanelStateContext : JsonSerializerContext{ }
