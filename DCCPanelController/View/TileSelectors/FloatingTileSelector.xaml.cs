using DCCPanelController.Models.ViewModel.Tiles;

namespace DCCPanelController.View.TileSelectors;

public partial class FloatingTileSelector {
    public FloatingTileSelector() {
        InitializeComponent();
        BindingContext = new FloatingTileSelectorViewModel();
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
        if (sender is DropGestureRecognizer { BindingContext: FloatingTileSelectorViewModel { } vm }) {
            var position = e.GetPosition(this);
            if (position is { } pos) {
                Console.WriteLine($"Drag Over: {sender?.GetType().Name} @ {pos.X}, {pos.Y}");
                vm.X = pos.X;
                vm.Y = pos.Y;
                vm.Width = Width;
                vm.Height = Height;
                OnPropertyChanged(vm.LayoutBounds);
            }
        }
    }

    private void DropGestureRecognizer_OnDrop(object? sender, DropEventArgs e) {
        Console.WriteLine($"DROPPED: {sender?.GetType().Name}");
        if (sender is DropGestureRecognizer { BindingContext: FloatingTileSelectorViewModel { } vm }) {
            var position = e.GetPosition(this);
            if (position is { } pos) {
                vm.X = pos.X;
                vm.Y = pos.Y;
                vm.Width = Width;
                vm.Height = Height;
                OnPropertyChanged(vm.LayoutBounds);
            }
        }
    }
}