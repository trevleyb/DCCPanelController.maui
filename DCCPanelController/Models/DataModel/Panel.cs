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
public partial class Panel : ObservableObject, IEntityID {

    [JsonConstructor]
    private Panel() {
        ResetColorsToDefaults();
    }

    public Panel(Panels panels) : this() {
        Panels = panels;
        Id = GenerateID();
    }
    
    [ObservableProperty] private int _sortOrder;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))] private string _id = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))] private string _description = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _cols = 27;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _rows = 18;
    [ObservableProperty] private ObservableCollection<Entity> _entities = [];
    
    [JsonIgnore] public Panels? Panels { get; set; }
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid();
    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);

    public Entity? GetEntityAtPosition(int x, int y) => Entities.FirstOrDefault(trk => trk.Col == x && trk.Row == y);
    public List<T> GetPanelEntitiesByType<T>() where T : Entity => Entities.OfType<T>().ToList() ?? [];
    public List<T> GetPanelEntitiesWithID<T>() where T : Entity, IEntityID => Entities.OfType<T>().Where(e => !string.IsNullOrEmpty(e.Id)).ToList() ?? [];
    public List<T> GetAllEntitiesByType<T>() where T : Entity => Panels?.SelectMany(panel => panel.GetPanelEntitiesByType<T>()).ToList() ?? [];
    public List<T> GetAllEntitiesWithID<T>() where T : Entity, IEntityID => Panels?.SelectMany(panel => panel.GetPanelEntitiesWithID<T>()).ToList() ?? [];
    public ButtonEntity? GetButtonEntity(string id) => GetAllEntitiesWithID<ButtonEntity>().FirstOrDefault(b => b.Id == id) ?? null;
    public TurnoutEntity? GetTurnoutEntity(string id) => GetAllEntitiesWithID<TurnoutEntity>().FirstOrDefault(b => b.Id == id) ?? null;

    [JsonIgnore]
    public string Title {
        get {
            if (string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Description)) return "DCC Panel Controller";
            if (!string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(Description)) return Id;
            if (string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Description)) return Description;
            return Id + $" - {Description}";
        }
    }

    public string GenerateID() => EntityID.NextPanelID(Panels ?? []);

    public Entity AddEntity(Entity entity) {
        entity.Parent = this;
        Entities.Add(entity);
        return entity;
    }

    public T CreateEntity<T>() where T : Entity {
        var entity = (T)Activator.CreateInstance(typeof(T), this)! ?? throw new InvalidOperationException();
        if (entity is IEntityID entityID) entityID.Id = entityID.GenerateID();
        return entity ?? throw new InvalidOperationException();
    }
    
    public T CreateEntityFrom<T>(T entity) where T : Entity {
        var cloned = entity.Clone() as T ?? throw new InvalidOperationException();
        if (cloned is IEntityID entityID) entityID.Id = entityID.GenerateID();
        return cloned ?? throw new InvalidOperationException();
    }
    
    public Panel Clone() {
        ArgumentNullException.ThrowIfNull(Panels);
        var clone = new Panel(panels: Panels) {
            Description = this.Description,
            SortOrder = this.SortOrder,
            Cols = this.Cols,
            Rows = this.Rows
        };
        CopyColorsTo(clone);
        foreach (var entity in this.Entities) {
            var entityClone = clone.CreateEntityFrom(entity);
            clone.AddEntity(entityClone);
        }
        return clone;
    }

    public void CheckEntityParents() {
        foreach (var entity in Entities)  entity.Parent ??= this;
    }

    public bool HasChanged(Panel original, Panel modified) {
        var originalJson = JsonSerializer.Serialize(this, JsonOptions.Options);
        var modifiedJson = JsonSerializer.Serialize(this, JsonOptions.Options);
        var originalHash = originalJson.GetHashCode();
        var modifiedHash = modifiedJson.GetHashCode();
        return originalHash != modifiedHash;
    }

    /// <summary>
    /// Serializes the current panel instance into a JSON string representation with predefined serialization options.
    /// </summary>
    /// <returns>A JSON string representation of the panel, including its properties and associated data.</returns>
    public string DownloadPanel() => JsonSerializer.Serialize<Panel>(this, JsonOptions.Options);
    
    /// <summary>
    /// Calculates the ratio of columns to rows in the format "x:y".
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