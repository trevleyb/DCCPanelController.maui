using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed record PropertyChange(object Entity, EditableField Field, object? OldValue, object? NewValue);

public interface IUndoService {
    void Push(ApplyTransaction tx);
    Task UndoAsync();
    Task RedoAsync();
}

public sealed class ApplyTransaction {
    public ApplyTransaction(IEnumerable<PropertyChange> changes) => Changes = changes.ToList();
    public IReadOnlyList<PropertyChange> Changes { get; }

    public Task CommitAsync() {
        foreach (var c in Changes) c.Field.Accessor.Set(c.Entity, c.NewValue);
        return Task.CompletedTask;
    }

    public Task RollbackAsync() {
        foreach (var c in Changes) c.Field.Accessor.Set(c.Entity, c.OldValue);
        return Task.CompletedTask;
    }
}

public sealed class DefaultUndoService : IUndoService {
    private readonly Stack<ApplyTransaction> _redo = new();
    private readonly Stack<ApplyTransaction> _undo = new();

    public void Push(ApplyTransaction tx) {
        _undo.Push(tx);
        _redo.Clear();
    }

    public async Task UndoAsync() {
        if (_undo.Count == 0) return;
        var tx = _undo.Pop();
        await tx.RollbackAsync();
        _redo.Push(tx);
    }

    public async Task RedoAsync() {
        if (_redo.Count == 0) return;
        var tx = _redo.Pop();
        await tx.CommitAsync();
        _undo.Push(tx);
    }
}