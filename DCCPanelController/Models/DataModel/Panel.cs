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
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))] private string _id = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Thumbnail))] private string _base64Image = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _cols = 27;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))] private string _description = string.Empty;
    [ObservableProperty] private ObservableCollection<Entity> _entities = [];
    [ObservableProperty] [JsonIgnore] private bool _isRefreshing;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _rows = 18;
    [ObservableProperty] private int _sortOrder;

    [JsonConstructor]
    private Panel() {
        ResetColorsToDefaults();
    }

    public Panel(Panels panels) : this() {
        Panels = panels;
        Id = NextID;
    }

    [JsonIgnore] public Panels? Panels { get; set; }
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid();
    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);

    [JsonIgnore] public ImageSource? Thumbnail =>
        string.IsNullOrWhiteSpace(Base64Image)
            ? null
            : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(Base64Image)));

    [JsonIgnore]
    public string Title {
        get {
            if (string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Description)) return "DCC Panel Controller";
            if (!string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Description)) return Id;
            if (string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Description)) return Description;
            return Id + $" - {Description}";
        }
    }

    [JsonIgnore] public ObservableCollection<Block> Blocks => Panels?.Profile?.Blocks ?? [];
    [JsonIgnore] public ObservableCollection<Route> Routes => Panels?.Profile?.Routes ?? [];
    [JsonIgnore] public ObservableCollection<Turnout> Turnouts => Panels?.Profile?.Turnouts ?? [];
    [JsonIgnore] public ObservableCollection<Signal> Signals => Panels?.Profile?.Signals ?? [];
    [JsonIgnore] public ObservableCollection<Sensor> Sensors => Panels?.Profile?.Sensors ?? [];
    [JsonIgnore] public ObservableCollection<Light> Lights => Panels?.Profile?.Lights ?? [];

    [JsonIgnore] public List<IEntityID> AllIDs => new List<IEntityID>(Panels ?? []) ?? [];
    [JsonIgnore] public string NextID => EntityID.GenerateNextID(Panels ?? [], "Panel");

    public Entity? GetEntityAtPosition(int x, int y) {
        return Entities.FirstOrDefault(trk => trk.Col == x && trk.Row == y);
    }

    public List<T> GetPanelEntitiesByType<T>() where T : Entity {
        return Entities.OfType<T>().ToList() ?? [];
    }

    public List<T> GetAllEntitiesByType<T>() where T : Entity {
        return Panels?.SelectMany(panel => panel.GetPanelEntitiesByType<T>()).ToList() ?? [];
    }

    public ActionButtonEntity? GetButtonEntity(string id) {
        return GetAllEntitiesByType<ActionButtonEntity>().FirstOrDefault(b => b.Id == id) ?? null;
    }

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

    public Block? Block(string id) {
        return Blocks.FirstOrDefault(x => x.Id != null && x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
    }

    public Route? Route(string id) {
        return Routes.FirstOrDefault(x => x.Id != null && x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
    }

    public Turnout? Turnout(string id) {
        return Turnouts.FirstOrDefault(x => x.Id != null && x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
    }

    public Signal? Signal(string id) {
        return Signals.FirstOrDefault(x => x.Id != null && x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
    }

    public Sensor? Sensor(string id) {
        return Sensors.FirstOrDefault(x => x.Id != null && x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
    }

    public Light? Light(string id) {
        return Lights.FirstOrDefault(x => x.Id != null && x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
    }

    public Entity AddEntity(Entity entity) {
        entity.Parent = this;
        Entities.Add(entity);
        return entity;
    }

    public T CreateEntity<T>() where T : Entity {
        var entity = (T)Activator.CreateInstance(typeof(T), this)! ?? throw new InvalidOperationException();
        if (entity is IEntityGeneratingID entityID) entityID.Id = entityID.NextID;
        return entity ?? throw new InvalidOperationException();
    }

    public T CreateEntityFrom<T>(T entity) where T : Entity {
        var cloned = entity.Clone() as T ?? throw new InvalidOperationException();
        if (cloned is IEntityGeneratingID entityID) entityID.Id = entityID.NextID;
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
            Description = Id,
            SortOrder = 999,
            Cols = Cols,
            Rows = Rows
        };
        CopyColorsTo(clone);
        return clone;
    }

    public Panel Clone(bool generateNewId = true) {
        ArgumentNullException.ThrowIfNull(Panels);
        var clone = new Panel(Panels) {
            Base64Image = Base64Image,
            Description = Description,
            SortOrder = SortOrder,
            Cols = Cols,
            Rows = Rows
        };
        if (!generateNewId) clone.Id = Id;

        CopyColorsTo(clone);
        foreach (var entity in Entities) {
            var entityClone = clone.CreateEntityFrom(entity);
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
    public string DownloadPanel() {
        return JsonSerializer.Serialize<Panel>(this, JsonOptions.Options);
    }

    public bool IsEqualTo(Panel panel) {
        var comparerOptions = new GenericComparerOptions {
            MaxDepth = 5,
            IncludePrivateProperties = false,
            CompareCollectionsOrdered = false, // For entity collections
            IgnoreProperties = { "Parent", "Navigation", "Panels", "Guid", "Id", "Base64Image" }
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
        return $"{x:0.#}:{y:0.#}";

        static double Gcd(double a, double b) {
            while (b != 0) {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}