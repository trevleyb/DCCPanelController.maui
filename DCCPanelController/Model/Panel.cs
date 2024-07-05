using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;

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

    public bool IsImageAvailable => Image != null;
    
    public ImageSource?  Image {
        get {
            if (string.IsNullOrEmpty(ImageAsBase64)) return null;
            var base64Stream = Convert.FromBase64String(ImageAsBase64);
            return ImageSource.FromStream(() => new MemoryStream(base64Stream));
        }
        set {
            ImageAsBase64 = null;
            if (value is StreamImageSource streamImageSource) {
                ImageAsBase64 = ConvertFromStreamAsync(streamImageSource.Stream).GetAwaiter().GetResult();
            }
        }
    }
    public string? ImageAsBase64 { get; set; } = null;
        
    private static async Task<string> ConvertFromStreamAsync(Func<CancellationToken, Task<Stream>> streamFunc) {
        await using Stream stream = await streamFunc(CancellationToken.None);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }

    public void Copy(Panel target) {
        Copy(this, target);
    }

    public Panel Copy(Panel source, Panel target) {
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

    public object Clone() {
        return Copy(this,new Panel());
    }
}

[JsonSerializable(typeof(List<Panel>))]
internal sealed partial class PanelStateContext : JsonSerializerContext{ }
