using CommunityToolkit.Maui.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public partial class ButtonTile(Entity entity, double gridSize) : Tile(entity, gridSize), ITileInteractive {
    private ButtonStateEnum State { get; set => SetField(ref field, value); }= ButtonStateEnum.Unknown;
    public override void CreateTile() {
        if (Entity is ButtonEntity button) {
            var svgImage = SvgImages.GetImage("button");
            var switchColor = State switch {
                ButtonStateEnum.On  => Colors.Green,
                ButtonStateEnum.Off => Colors.Red,
                _                   => Colors.Gray
            };
            svgImage.SetAttribute(SvgElementType.Button, switchColor);
            
            var image = new Image();
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
            SetContent(image);
        }
    }

    public void Interact() {
        State = State switch {
            ButtonStateEnum.Unknown => ButtonStateEnum.On,
            ButtonStateEnum.On      => ButtonStateEnum.Off,
            ButtonStateEnum.Off     => ButtonStateEnum.On,
            _                       => ButtonStateEnum.Unknown
        };
        CreateTile();
        OnPropertyChanged(nameof(ImageSource));
    }
    
}