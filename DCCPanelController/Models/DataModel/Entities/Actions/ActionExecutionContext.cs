namespace DCCPanelController.Models.DataModel.Entities.Actions;

public enum StateChangeSource {
    External, // From physical layout/user interaction
    Internal, // From cascading actions
}

public class ActionExecutionContext {
    private readonly HashSet<string> _currentCascade = new();
    private readonly int             _maxDepth;
    private          int             _currentDepth;

    public ActionExecutionContext(int maxDepth = 10) => _maxDepth = maxDepth;

    public bool CanCascade(string entityId) => !_currentCascade.Contains(entityId) && _currentDepth < _maxDepth;

    public IDisposable BeginCascade(string entityId) {
        if (!CanCascade(entityId)) {
            return new NoOpDisposable();
        }

        _currentCascade.Add(entityId);
        _currentDepth++;

        return new CascadeScope(this, entityId);
    }

    private class CascadeScope : IDisposable {
        private readonly ActionExecutionContext _context;
        private readonly string                 _entityId;

        public CascadeScope(ActionExecutionContext context, string entityId) {
            _context = context;
            _entityId = entityId;
        }

        public void Dispose() {
            _context._currentCascade.Remove(_entityId);
            _context._currentDepth--;
        }
    }

    private class NoOpDisposable : IDisposable {
        public void Dispose() { }
    }
}