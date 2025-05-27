using System.Threading.Tasks;

namespace YourAppNamespace.ViewModels
{
    public interface IPropertiesViewModel
    {
        string Title { get; }
        Task ApplyChangesAsync();
        // You can add more common properties or methods here, 
        // e.g., IsDirty, CancelChangesAsync, etc.

        // This method would be responsible for creating the actual UI for the properties.
        // The View returned would be placed into the PropertySheetPage or PropertyPopup.
        View CreatePropertiesView(); 
    }
}