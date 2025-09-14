using DCCPanelController.Models.DataModel;

namespace DCCPanelController.View.Selectors;

public class ThumbnailOrPanelTemplateSelector : DataTemplateSelector {
    public DataTemplate? ThumbnailTemplate { get; set; }
    public DataTemplate? PanelTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) {
        if (item is Panel panel) {
            return panel.Thumbnail != null ? ThumbnailTemplate! : PanelTemplate!;
        }

        // Fallback
        return PanelTemplate!;
    }
}