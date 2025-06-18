namespace DCCPanelController.Models.DataModel.Helpers;

public class PanelChangeDetectorOptions {
    public int MaxDepth { get; set; } = 10;
    public HashSet<string> SkipProperties { get; set; } = new() { "Parent", "Navigation", "BindingContext", "Handler", "Dispatcher" };
    public HashSet<Type> SkipTypes { get; set; } = new() { typeof(INavigation), typeof(Page) };
    public bool IncludePrivateProperties { get; set; } = false;
    public bool StrictTypeChecking { get; set; } = true;
}
