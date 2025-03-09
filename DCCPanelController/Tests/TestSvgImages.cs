using DCCPanelController.Models.ViewModel.ImageManager;

namespace DCCPanelController.Tests;

public static class TestSvgImages {

    public static void TestGetImages() {
        var straight000 = SvgImages.GetImage("straight",0);
        var straight090 = SvgImages.GetImage("straight",90);
        var straight180 = SvgImages.GetImage("straight",180);
        var straight270 = SvgImages.GetImage("straight",270);

        var straightN = SvgImages.GetImage("straight",SvgDirection.North);
        var straightE = SvgImages.GetImage("straight",SvgDirection.East);
        var straightS = SvgImages.GetImage("straight",SvgDirection.South);
        var straightW = SvgImages.GetImage("straight",SvgDirection.West);
    }
    
}