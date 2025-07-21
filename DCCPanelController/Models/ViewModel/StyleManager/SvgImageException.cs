using DCCPanelController.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.ViewModel.StyleManager;

public class SvgImageException : Exception {
    public SvgImageException(string? message) : base(message) {
        LogHelper.CreateLogger("SvgImage").LogError("ImageException: {Message}", message);
    }
}