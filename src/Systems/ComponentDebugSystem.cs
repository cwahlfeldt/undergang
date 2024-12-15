using Game.Components;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Game
{
    public partial class ComponentDebugSystem : System
    {
        private Control _componentViewer;
        private Dictionary<int, RichTextLabel> _entityLabels = new();
        private bool _showComponents = false;

        public override void Initialize()
        {
            SetupComponentViewer();
            ToggleComponentViewer();

            // Subscribe to component changes
            Events.ComponentChanged += OnComponentChanged;
        }

        private void SetupComponentViewer()
        {
            // Create a Control node for the component viewer
            _componentViewer = new Control
            {
                Name = "Component Viewer",
                Position = new Vector2(10, 10),
                Size = new Vector2(300, 600)
            };

            // Add it to the debug node
            var canvas = new CanvasLayer { Name = "Debug Canvas" };
            Entities.GetRootNode().AddChild(canvas);
            canvas.AddChild(_componentViewer);

            // Add a background panel
            var panel = new PanelContainer
            {
                Name = "Background",
                Size = _componentViewer.Size
            };
            _componentViewer.AddChild(panel);

            // Add a scroll container
            var scroll = new ScrollContainer
            {
                Name = "Scroll",
                Size = panel.Size
            };
            panel.AddChild(scroll);

            // Add a vertical container for entity labels
            var container = new VBoxContainer
            {
                Name = "Container",
                Size = scroll.Size
            };
            scroll.AddChild(container);

            _componentViewer.Visible = _showComponents;
        }

        public void ToggleComponentViewer()
        {
            _showComponents = !_showComponents;
            _componentViewer.Visible = _showComponents;

            if (_showComponents)
                UpdateAllEntityComponents();
            else
                ClearComponentViewer();
        }

        private void UpdateAllEntityComponents()
        {
            ClearComponentViewer();
            var container = _componentViewer.GetNode<VBoxContainer>("Background/Scroll/Container");

            foreach (var entity in Entities.Query<Unit>())
            {
                var label = CreateEntityLabel(entity);
                container.AddChild(label);
                _entityLabels[entity.Id] = label;
            }
        }

        private RichTextLabel CreateEntityLabel(Entity entity)
        {
            var label = new RichTextLabel
            {
                Name = $"Entity_{entity.Id}",
                BbcodeEnabled = true,
                FitContent = true,
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                CustomMinimumSize = new Vector2(280, 0),
                Text = FormatEntityComponents(entity)
            };
            return label;
        }

        private string FormatEntityComponents(Entity entity)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[color=yellow]Entity {entity.Id}[/color]");

            // Get all components using reflection
            var components = entity.GetType()
                .GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(entity) as Dictionary<Type, object>;

            if (components != null)
            {
                foreach (var (type, component) in components)
                {
                    sb.AppendLine($"[color=aqua]{type.Name}[/color]");
                    var properties = component.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(component);
                        sb.AppendLine($"  {prop.Name}: {value}");
                    }
                }
            }

            return sb.ToString();
        }

        private void OnComponentChanged(Type componentType, object component)
        {
            if (!_showComponents) return;

            // Find the entity that contains this component
            var entity = Entities.GetEntities().Values.FirstOrDefault(e =>
            {
                var components = e.GetType()
                    .GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(e) as Dictionary<Type, object>;
                return components?.ContainsValue(component) ?? false;
            });

            if (entity != null && _entityLabels.TryGetValue(entity.Id, out var label))
            {
                label.Text = FormatEntityComponents(entity);
            }
        }

        private void ClearComponentViewer()
        {
            var container = _componentViewer.GetNode<VBoxContainer>("Background/Scroll/Container");
            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }
            _entityLabels.Clear();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Events.ComponentChanged -= OnComponentChanged;
        }
    }
}