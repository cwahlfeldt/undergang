using Game.Components;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game
{
    public partial class ComponentDebugSystem : System
    {
        private Control _debugPanel;
        private VBoxContainer _container;
        private Dictionary<int, RichTextLabel> _entityLabels = [];
        private bool _isVisible;

        // Matches existing material color scheme
        private static readonly Color HeaderColor = Colors.Yellow;
        private static readonly Color ComponentColor = Colors.Aqua;
        private static readonly Color PropertyColor = Colors.LightGreen;

        public override void Initialize()
        {
            SetupDebugPanel();
            Toggle();
            Events.ComponentChanged += OnComponentChanged;
        }

        private void SetupDebugPanel()
        {
            var canvas = new CanvasLayer { Name = "Debug Layer" };
            Entities.GetRootNode().AddChild(canvas);

            _debugPanel = new PanelContainer
            {
                Name = "Debug Panel",
                Position = new Vector2(10, 10),
                CustomMinimumSize = new Vector2(300, 600),
                Theme = CreateDebugTheme()
            };
            canvas.AddChild(_debugPanel);

            var scroll = new ScrollContainer
            {
                CustomMinimumSize = _debugPanel.CustomMinimumSize,
                VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways
            };
            _debugPanel.AddChild(scroll);

            _container = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(280, 0)
            };
            scroll.AddChild(_container);

            _debugPanel.Hide();
        }

        private Theme CreateDebugTheme()
        {
            var theme = new Theme();
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                BorderWidthLeft = 1,
                BorderWidthTop = 1,
                BorderWidthRight = 1,
                BorderWidthBottom = 1,
                BorderColor = new Color(0.3f, 0.3f, 0.3f),
                CornerRadiusBottomLeft = 5,
                CornerRadiusTopRight = 5,
                CornerRadiusTopLeft = 5,
                CornerRadiusBottomRight = 5,
                ContentMarginLeft = 5,
                ContentMarginTop = 5,
                ContentMarginRight = 5,
                ContentMarginBottom = 5
            };
            theme.SetStylebox("panel", "PanelContainer", style);
            return theme;
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            _debugPanel.Visible = _isVisible;

            if (_isVisible)
                UpdateDebugInfo();
            else
                ClearDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            ClearDebugInfo();

            foreach (var entity in Entities.Query<Unit>().OrderBy(e => e.Id))
            {
                var label = new RichTextLabel
                {
                    Name = $"Entity_{entity.Id}",
                    BbcodeEnabled = true,
                    FitContent = true,
                    CustomMinimumSize = new Vector2(280, 0)
                };

                label.Text = FormatEntity(entity);
                _container.AddChild(label);
                _entityLabels[entity.Id] = label;
            }
        }

        private string FormatEntity(Entity entity)
        {
            var components = entity.GetType()
                .GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(entity) as Dictionary<Type, object>;

            if (components == null)
                return string.Empty;

            var name = entity.Has<Name>() ? $" - {entity.Get<Name>()}" : "";
            var text = $"[color=#{HeaderColor.ToHtml()}]Entity {entity.Id}{name}[/color]\n";

            foreach (var (type, component) in components.OrderBy(c => c.Key.Name))
            {
                text += $"\n[color=#{ComponentColor.ToHtml()}]▼ {type.Name}[/color]";

                foreach (var prop in component.GetType().GetProperties().OrderBy(p => p.Name))
                {
                    var value = prop.GetValue(component);
                    var displayValue = FormatValue(value);
                    text += $"\n  [color=#{PropertyColor.ToHtml()}]{prop.Name}:[/color] {displayValue}";
                }
            }

            return text;
        }

        private string FormatValue(object value)
        {
            return value switch
            {
                null => "null",
                Vector3I v => $"({v.X}, {v.Y}, {v.Z})",
                IEnumerable<object> list => $"[{string.Join(", ", list)}]",
                _ => value.ToString()
            };
        }

        private void OnComponentChanged(int id, Type _, object __)
        {
            if (!_isVisible) return;

            if (_entityLabels.TryGetValue(id, out var label))
            {
                var entity = Entities.Query<Unit>().FirstOrDefault(e => e.Id == id);
                if (entity != null)
                    label.Text = FormatEntity(entity);
            }
        }

        private void ClearDebugInfo()
        {
            foreach (Node child in _container.GetChildren())
                child.QueueFree();

            _entityLabels.Clear();
        }

        public override void Cleanup()
        {
            Events.ComponentChanged -= OnComponentChanged;
        }
    }
}