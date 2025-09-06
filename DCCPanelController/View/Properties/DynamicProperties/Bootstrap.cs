namespace DCCPanelController.View.Properties.DynamicProperties;

public enum AppMode { Edit, Run }

public static class Bootstrap {
    public static FormContext CreateForm(IEnumerable<object> selection) {
        var extractor = new EditableExtractorCache();
        var renderers = new PropertyRendererRegistry();
        PropertyRenderers.RegisterDefaults(renderers);

        var validator = new CompositeValidator([
            new PropertyRendererRules.RequiredRule(),
            new PropertyRendererRules.RangeRule(),
            new PropertyRendererRules.RegexRule()
        ]);

        var equality = new DefaultEqualityPolicy();
        var undo = new DefaultUndoService();
        var kindResolver = new EditableExtractorResolver();

        return new FormContext(selection, extractor, renderers, validator, equality, undo, kindResolver, AppMode.Edit);
    }
}