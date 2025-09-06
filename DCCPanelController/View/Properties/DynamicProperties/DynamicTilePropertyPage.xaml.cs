using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.View.Properties.DynamicProperties {
    public partial class DynamicTilePropertyPopupContent : ContentView {
        public DynamicTilePropertyPopupContent() {
            InitializeComponent();
            WireButtons();
        }

        // ===== Bindable input: a collection of ITile =====
        public static readonly BindableProperty TilesSourceProperty =
            BindableProperty.Create(
                nameof(TilesSource),
                typeof(IEnumerable<ITile>),
                typeof(DynamicTilePropertyPopupContent),
                defaultValue: null,
                propertyChanged: OnTilesSourceChanged);

        public IEnumerable<ITile>? TilesSource {
            get => (IEnumerable<ITile>?)GetValue(TilesSourceProperty);
            set => SetValue(TilesSourceProperty, value);
        }

        // Optional: expose the built FormContext
        public FormContext? Form => _form;

        // Events so the parent popup can react
        public event EventHandler? Applied;
        public event EventHandler? Closed;

        private FormContext? _form;
        private IUndoService _undo = new DefaultUndoService(); // from MauiAdapters

        private static async void OnTilesSourceChanged(BindableObject bindable, object oldValue, object newValue) {
            var view = (DynamicTilePropertyPopupContent)bindable;
            await view.RebuildAsync();
        }

        private async System.Threading.Tasks.Task RebuildAsync() {
            PropertyHost.Children.Clear();
            var tiles = TilesSource?.ToList();
            if (tiles == null || tiles.Count == 0)
                return;

            // Build selection from ITile.Entity
            var selection = tiles.Select(t => (object)t.Entity);

            // Use the helper bootstrap that registers renderers + rules
            _form = Bootstrap.CreateForm(selection);
            await _form.ValidateAsync(); // optional validation on load

            // Render rows into the host
            foreach (var row in _form.Rows) {
                if (_form.GetRendererView(row) is Microsoft.Maui.Controls.View v)
                    PropertyHost.Children.Add(v);
            }
        }

        private void WireButtons() {
            BtnValidate.Clicked += async (_, __) => {
                if (_form == null) return;
                var summary = await _form.ValidateAsync();
                if (summary.HasErrors)
                    // TODO: Fix the Popup
                    await Application.Current!.MainPage!.DisplayAlert("Validation", "Please fix errors.", "OK");
                else
                    // TODO: Fix the Popup
                    await Application.Current!.MainPage!.DisplayAlert("Validation", "All good.", "OK");
            };

            BtnApply.Clicked += async (_, __) => {
                if (_form == null) return;

                // Important: ensure PropertyChange uses row.Field (already fixed in your FormContext)
                var ok = await _form.ApplyAsync(requireAtomic: false);
                if (!ok) {
                    // TODO: Fix the Popup
                    await Application.Current!.MainPage!.DisplayAlert("Apply", "Nothing to apply or failed.", "OK");
                    return;
                }

                Applied?.Invoke(this, EventArgs.Empty);
            };

            BtnUndo.Clicked += async (_, __) => {
                // Simple in-memory undo (applies last transaction)
                await _undo.UndoAsync();

                // Refresh UI to reflect undo’d values (rebuild from TilesSource)
                await RebuildAsync();
            };

            BtnClose.Clicked += (_, __) => Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}