using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View;

public partial class TileSelector {
    public TileSelector() {
        InitializeComponent();
        BindingContext = new TileSelectorViewModel();
    }

    private void SymbolDragStarting(object? sender, DragStartingEventArgs e) {
        Console.WriteLine($"Drag Starting: {sender?.GetType().Name}");
        if (sender is DragGestureRecognizer { BindingContext: Tile { } tile }) {
            if (e.Data.Properties is { } properties) {
                properties.Add("Tile", tile);
                properties.Add("Source", "Symbol");
            }
        }
    }

    private void DragGestureRecognizer_OnDragStarting(object? sender, DragStartingEventArgs e) {
        Console.WriteLine($"Drag Starting: {sender?.GetType().Name}");
    }

    private void DropGestureRecognizer_OnDragOver(object? sender, DragEventArgs e) {
        Console.WriteLine($"Drag Over: {sender?.GetType().Name}");
        if (sender is DropGestureRecognizer { BindingContext: TileSelectorViewModel { } vm }) {
            var position = e.GetPosition(this);
            if (position is { } pos) {
                Console.WriteLine($"Drag Over: {sender?.GetType().Name} @ {pos.X}, {pos.Y}");
                vm.X = pos.X;
                vm.Y = pos.Y;
                vm.Width = this.Width;
                vm.Height = this.Height;
                OnPropertyChanged(vm.LayoutBounds);
            }
        }
    }

    private void DropGestureRecognizer_OnDrop(object? sender, DropEventArgs e) {
        Console.WriteLine($"DROPPED: {sender?.GetType().Name}");
        if (sender is DropGestureRecognizer { BindingContext: TileSelectorViewModel { } vm }) {
            var position = e.GetPosition(this);
            if (position is { } pos) {
                Console.WriteLine($"DROPPED: {sender?.GetType().Name} @ {pos.X}, {pos.Y}");
                vm.X = pos.X;
                vm.Y = pos.Y;
                vm.Width = this.Width;
                vm.Height = this.Height;
                OnPropertyChanged(vm.LayoutBounds);
            }
        }
    }
}