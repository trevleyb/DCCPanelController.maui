using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
[DebuggerDisplay("Panel: {Id}")]
public partial class Panel : ObservableObject, IEntityGeneratingID {
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int                          _cols     = 27;
    [ObservableProperty]                                                private ObservableCollection<Entity> _entities = [];
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))]      private string                       _id       = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int                          _rows     = 18;
    [ObservableProperty]                                                private int                          _sortOrder;

    [JsonConstructor]
    private Panel() => ResetColorsToDefaults();

    public Panel(Panels panels) : this() {
        Panels = panels;
        Id = NextID();
    }

    [JsonIgnore] public Panels? Panels { get; set; }
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid();
    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);

    [JsonIgnore]
    public string Title => string.IsNullOrEmpty(Id) ? "DCC Panel Controller" : Id;

    [JsonIgnore] public ObservableCollection<Block> Blocks => Panels?.Profile?.Blocks ?? [];
    [JsonIgnore] public ObservableCollection<Route> Routes => Panels?.Profile?.Routes ?? [];
    [JsonIgnore] public ObservableCollection<Turnout> Turnouts => Panels?.Profile?.Turnouts ?? [];
    [JsonIgnore] public ObservableCollection<Signal> Signals => Panels?.Profile?.Signals ?? [];
    [JsonIgnore] public ObservableCollection<Sensor> Sensors => Panels?.Profile?.Sensors ?? [];
    [JsonIgnore] public ObservableCollection<Light> Lights => Panels?.Profile?.Lights ?? [];

    public string NextID(Panel? targetPanel = null) {
        targetPanel ??= this;
        var nextID = EntityHelper.GenerateID(EntityHelper.GetAllEntitiesByType<Panel>(targetPanel), "Panel");
        return nextID;
    }

    public List<IEntityID> AllIDs() {
        var allIDs = new List<IEntityID>(Panels ?? []) ?? [];
        return allIDs;
    }

    public Entity? GetEntityAtPosition(int x, int y) => Entities.FirstOrDefault(trk => trk.Col == x && trk.Row == y);

    public List<T> GetPanelEntitiesByType<T>() where T : Entity => Entities.OfType<T>().ToList() ?? [];

    public List<T> GetAllEntitiesByType<T>() where T : Entity => Panels?.SelectMany(panel => panel.GetPanelEntitiesByType<T>()).ToList() ?? [];

    public ActionButtonEntity? GetButtonEntity(string id) => GetAllEntitiesByType<ActionButtonEntity>().FirstOrDefault(b => b.Id == id) ?? null;

    public TurnoutEntity? GetTurnoutEntity(string id) {
        var allEntitiesWithID = GetAllEntitiesByType<TurnoutEntity>();
        var foundItem = allEntitiesWithID.FirstOrDefault(b => b.TurnoutID == id) ?? null;
        return foundItem;
    }

    public TurnoutEntity? GetTurnoutEntityByRef(string id) {
        var allEntitiesWithID = GetAllEntitiesByType<TurnoutEntity>();
        var foundItem = allEntitiesWithID.FirstOrDefault(b => b?.Turnout?.Id == id) ?? null;
        return foundItem;
    }

    public Block? Block(string name) => Blocks.FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public Route? Route(string name) => Routes.FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public Turnout? Turnout(string name) => Turnouts.FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public Signal? Signal(string name) => Signals.FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public Sensor? Sensor(string name) => Sensors.FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public Light? Light(string name) => Lights.FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public Entity AddEntity(Entity entity) {
        entity.Parent = this;
        Entities.Add(entity);
        return entity;
    }

    public T CreateEntity<T>() where T : Entity {
        var entity = (T)Activator.CreateInstance(typeof(T), this)! ?? throw new InvalidOperationException();
        if (entity is IEntityGeneratingID entityID) entityID.Id = entityID.NextID();
        return entity ?? throw new InvalidOperationException();
    }

    public T CreateEntityFrom<T>(T entity, Panel? realParent = null, bool generateNextID = true) where T : Entity {
        var cloned = entity.Clone() as T ?? throw new InvalidOperationException();
        if (realParent != null) cloned.Parent = realParent; // Set the Panel that owns this entity
        if (cloned is IEntityGeneratingID clonedID && entity is IEntityGeneratingID entityID) {
            clonedID.Id = generateNextID ? clonedID.NextID(realParent) : entityID.Id;
        }
        return cloned ?? throw new InvalidOperationException();
    }

    /// <summary>
    ///     This is a special Clone that does not clone the child elements
    ///     and it does not add this to the parent but references the parent
    /// </summary>
    public Panel CloneEmptyPanel(string id) {
        ArgumentNullException.ThrowIfNull(Panels);
        var clone = new Panel(Panels) {
            Base64Image = string.Empty,
            Id = id,
            SortOrder = 999,
            Cols = Cols,
            Rows = Rows,
        };
        CopyColorsTo(clone);
        return clone;
    }

    public Panel Clone(bool generateNewId = true) {
        ArgumentNullException.ThrowIfNull(Panels);
        var clone = new Panel(Panels) {
            Base64Image = Base64Image,
            SortOrder = SortOrder,
            Cols = Cols,
            Rows = Rows,
        };
        if (!generateNewId) clone.Id = Id;

        CopyColorsTo(clone);
        foreach (var entity in Entities) {
            var entityClone = clone.CreateEntityFrom(entity, clone, generateNewId);
            clone.AddEntity(entityClone);
        }
        return clone;
    }

    public void CheckEntityParents() {
        foreach (var entity in Entities) entity.Parent ??= this;
    }

    /// <summary>
    ///     Serializes the current panel instance into a JSON string representation with predefined serialization options.
    /// </summary>
    /// <returns>A JSON string representation of the panel, including its properties and associated data.</returns>
    public string DownloadPanel() => JsonSerializer.Serialize<Panel>(this, JsonOptions.Options);

    public bool IsEqualTo(Panel panel) {
        var comparerOptions = new GenericComparerOptions {
            MaxDepth = 5,
            IncludePrivateProperties = false,
            CompareCollectionsOrdered = false, // For entity collections
            IgnoreProperties = { "Parent", "Navigation", "Panels", "Guid", "Base64Image" },
        };

        return GenericComparer.AreEqual(this, panel, comparerOptions);
    }

    /// <summary>
    ///     Calculates the ratio of columns to rows in the format "x:y".
    /// </summary>
    /// <param name="col">The number of columns.</param>
    /// <param name="row">The number of rows.</param>
    /// <returns>A string representing the calculated ratio in the format "x:y".</returns>
    private static string CalculateRatio(int col, int row) {
        var gcd = Gcd(col, row);
        var x = col / gcd;
        var y = row / gcd;
        return$"{x:0.#}:{y:0.#}";

        static double Gcd(double a, double b) {
            while (b != 0) {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }

    #region Manage the Thumbnail
    private ImageSource? _thumbnailImageSource;

    // If we set the Base64Image property, we need to update the Thumbnail property
    // ----------------------------------------------------------------------------
    public string Base64Image {
        get;
        set {
            _thumbnailImageSource = null;
            field = value;
            OnPropertyChanged();                  // Notifies Base64Image
            OnPropertyChanged(nameof(Thumbnail)); // Also notify Thumbnail bindings
        }
    } = string.Empty;

    // Cache the Thumbnail property so we dont recreate constantly
    // -----------------------------------------------------------
    [JsonIgnore] public bool HasThumbnail => !string.IsNullOrWhiteSpace(Base64Image);
    [JsonIgnore] public bool HasNoThumbnail => string.IsNullOrWhiteSpace(Base64Image);

    [JsonIgnore] public ImageSource? Thumbnail {
        get {
            if (string.IsNullOrWhiteSpace(Base64Image)) return null;
            return _thumbnailImageSource ??= CreateThumbnailImageSource();

            ImageSource CreateThumbnailImageSource() {
                try {
                    var bytes = Convert.FromBase64String(Base64Image);
                    return ImageSource.FromStream(() => new MemoryStream(bytes, false));
                } catch {
                    return null!;
                }
            }
        }
    }
    #endregion
}