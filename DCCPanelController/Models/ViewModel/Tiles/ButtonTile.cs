using CommunityToolkit.Maui.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class ButtonTile(Entity entity, double gridSize) : Tile(entity, gridSize), ITileInteractive {
    private ButtonStateEnum State { get; set => SetField(ref field, value); }= ButtonStateEnum.Unknown;
    public override void CreateTile() {
        if (Entity is ButtonEntity button) {
            var svgImage = SvgImages.GetImage("button", button.Rotation);
            svgImage.SetAttribute(SvgElementType.Button, State switch {
                ButtonStateEnum.On  => button.Parent?.ButtonOnColor ?? Colors.Green,
                ButtonStateEnum.Off => button.Parent?.ButtonOffColor ?? Colors.Red,
                _                   => button.Parent?.ButtonColor ?? Colors.Gray
            });
            svgImage.SetAttribute(SvgElementType.ButtonOutline, State switch {
                ButtonStateEnum.On  => button.Parent?.ButtonOnBorder ?? Colors.Black,
                ButtonStateEnum.Off => button.Parent?.ButtonOffBorder ?? Colors.Black,
                _                   => button.Parent?.ButtonBorder ?? Colors.Black
            });

            var image = new Image();
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
            SetContent(image);
        }
    }

    public void PrimaryInteract() {
        Console.WriteLine("PrimaryInteract");
        State = State switch {
            ButtonStateEnum.Unknown => ButtonStateEnum.On,
            ButtonStateEnum.On      => ButtonStateEnum.Off,
            ButtonStateEnum.Off     => ButtonStateEnum.On,
            _                       => ButtonStateEnum.Unknown
        };
        CreateTile();
        OnPropertyChanged(nameof(ImageSource));
    }
    
    public void SecondaryInteract() {
        Console.WriteLine("SecondaryInteract");
        Entity.IsEnabled = !Entity.IsEnabled;
        OnPropertyChanged(nameof(IsEnabled));
    }
}