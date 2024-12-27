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
        private RichTextLabel _debugText;
        private HashSet<int> _selected = [];

        public override void Initialize()
        {
            _debugText = new RichTextLabel { Position = new Vector2(10, 10), Size = new Vector2(300, 600) };
            Entities.GetRootNode().AddChild(_debugText);
            Events.Instance.UnitHover += OnUnitHover;
            Events.Instance.UnitUnhover += OnUnitUnhover;
            _debugText.BbcodeEnabled = true;
            UpdateDebug();
        }

        private void OnUnitHover(Entity unit)
        {
            GD.Print("yeah");
            if (Input.IsMouseButtonPressed(MouseButton.Right))
            {
                if (_selected.Contains(unit.Id))
                    _selected.Remove(unit.Id);
                else
                    _selected.Add(unit.Id);
                UpdateDebug();
            }
        }

        private void OnUnitUnhover(Entity unit) { }

        private void UpdateDebug()
        {
            if (_selected.Count == 0)
            {
                _debugText.Text = "Right click units to inspect";
                return;
            }

            string text = "";
            var entities = _selected.Select(id => Entities.GetEntity(id)).Where(e => e != null).ToList();

            foreach (var entity in entities)
            {
                var components = entity.GetType()
                    .GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(entity) as Dictionary<Type, object>;

                text += $"[color=yellow]Entity {entity.Id}[/color]\n";

                foreach (var (type, component) in components.OrderBy(c => c.Key.Name))
                {
                    if (type == typeof(Instance)) continue;

                    text += $"\n[color=aqua]{type.Name}[/color]";
                    foreach (var prop in type.GetProperties())
                    {
                        var value = prop.GetValue(component);
                        var diff = entities.Count > 1 && entities.Any(e =>
                        {
                            if (e.Id == entity.Id) return false;
                            var method = typeof(Entity).GetMethod("Get").MakeGenericMethod(type);
                            var other = method.Invoke(e, null);
                            return other == null || !Equals(prop.GetValue(other), value);
                        });
                        text += $"\n  [color={(diff ? "red" : "lime")}]{prop.Name}:[/color] {value}";
                    }
                }
                text += "\n\n";
            }
            _debugText.Text = text;
        }

        public override void Cleanup()
        {
            Events.Instance.UnitHover -= OnUnitHover;
            Events.Instance.UnitUnhover -= OnUnitUnhover;
            _debugText?.QueueFree();
        }
    }
}