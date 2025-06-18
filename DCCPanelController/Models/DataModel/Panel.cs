using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
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
public partial class Panel : ObservableObject, IEntityID {
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Thumbnail))] private string _base64Image = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))] private string _description = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _cols = 27;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _rows = 18;
    [ObservableProperty] private ObservableCollection<Entity> _entities = [];

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))] private string _id = string.Empty;
    [ObservableProperty] private int _sortOrder;

    [JsonIgnore] public Panels? Panels { get; set; }
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid();
    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);
    
    [JsonConstructor]
    private Panel() {
        ResetColorsToDefaults();
    }

    public Panel(Panels panels) : this() {
        Panels = panels;
        Id = NextID;
    }

    [JsonIgnore]
    public string Title {
        get {
            if (string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Description)) return "DCC Panel Controller";
            if (!string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Description)) return Id;
            if (string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Description)) return Description;
            return Id + $" - {Description}";
        }
    }

    [JsonIgnore]
    public ImageSource? Thumbnail =>
        string.IsNullOrWhiteSpace(Base64Image)
            ? null
            : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(Base64Image)));

    public ObservableCollection<Block> Blocks => Panels?.Profile?.Blocks ?? [];
    public ObservableCollection<Route> Routes => Panels?.Profile?.Routes ?? [];
    public ObservableCollection<Turnout> Turnouts => Panels?.Profile?.Turnouts ?? [];
    public ObservableCollection<Signal> Signals => Panels?.Profile?.Signals ?? [];
    public ObservableCollection<Sensor> Sensors => Panels?.Profile?.Sensors ?? [];
    public ObservableCollection<Light> Lights => Panels?.Profile?.Lights ?? [];

    [JsonIgnore]
    public List<IEntityID> AllIDs => new List<IEntityID>(Panels ?? []) ?? [];
    public string NextID => EntityID.GenerateNextID(Panels ?? [],"Panel");

    public Entity? GetEntityAtPosition(int x, int y) {
        return Entities.FirstOrDefault(trk => trk.Col == x && trk.Row == y);
    }

    public List<T> GetPanelEntitiesByType<T>() where T : Entity {
        return Entities.OfType<T>().ToList() ?? [];
    }
    
    public List<T> GetPanelEntitiesWithID<T>() where T : Entity, IEntityID {
        return Entities.OfType<T>().Where(e => !string.IsNullOrEmpty(e.Id)).ToList() ?? [];
    }
    
    public List<T> GetAllEntitiesByType<T>() where T : Entity {
        return Panels?.SelectMany(panel => panel.GetPanelEntitiesByType<T>()).ToList() ?? [];
    }
    
    public List<T> GetAllEntitiesWithID<T>() where T : Entity, IEntityID {
        return Panels?.SelectMany(panel => panel.GetPanelEntitiesWithID<T>()).ToList() ?? [];
    }

    public ButtonEntity? GetButtonEntity(string id) {
        return GetAllEntitiesWithID<ButtonEntity>().FirstOrDefault(b => b.Id == id) ?? null;
    }

    public TurnoutEntity? GetTurnoutEntity(string id) {
        return GetAllEntitiesWithID<TurnoutEntity>().FirstOrDefault(b => b.Id == id) ?? null;
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
        if (entity is IEntityID entityID) entityID.Id = entityID.NextID;
        return entity ?? throw new InvalidOperationException();
    }

    public T CreateEntityFrom<T>(T entity) where T : Entity {
        var cloned = entity.Clone() as T ?? throw new InvalidOperationException();
        if (cloned is IEntityID entityID) entityID.Id = entityID.NextID;
        return cloned ?? throw new InvalidOperationException();
    }

    public Panel Clone() {
        ArgumentNullException.ThrowIfNull(Panels);
        var clone = new Panel(Panels) {
            Description = Description,
            SortOrder = SortOrder,
            Base64Image = Base64Image,
            Cols = Cols,
            Rows = Rows
        };
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