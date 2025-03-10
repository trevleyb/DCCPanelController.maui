namespace DCCPanelController.Models.ViewModel.StyleManager;

public class SvgImageException : Exception {
    public SvgImageException(string? message) : base(message) {
        Console.WriteLine(message);
    }
}