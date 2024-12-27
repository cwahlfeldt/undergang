using System.Collections.Generic;
using System.Linq;
using Game.Components;
using Godot;

namespace Game
{
    public class UISystem : System
    {
        private readonly Stack<Control> _hearts = [];
        private Control _uiNode;
        private const int HEART_SIZE = 32;
        private const int HEART_SPACING = 8;
        private Camera3D _camera;

        // public override void Initialize()
        // {
        //     _uiNode = Entities.GetRootNode().GetNode<Control>("UI/SubViewportContainer/SubViewport/Control");
        //     _camera = Entities.GetRootNode().GetNode<Camera3D>("Camera");

        //     var player = Entities.Query<Player>().FirstOrDefault();
        //     var playerHealth = player.Get<Health>();

        //     for (int i = 0; i < playerHealth; i++)
        //     {
        //         AddHeart(i * (HEART_SIZE + HEART_SPACING));
        //     }

        //     // Connect to input events
        //     Input.MouseButtonPressed += OnMouseButtonPressed;
        // }

        // private void OnMouseButtonPressed(MouseButton button)
        // {
        //     if (button != MouseButton.Right)
        //         return;

        //     var mousePos = GetMousePosition();
        //     var entity = GetEntityUnderMouse(mousePos);

        //     if (entity != null)
        //     {
        //         Events.EntitySelected?.Invoke(entity);
        //     }
        // }

        // private Vector2 GetMousePosition()
        // {
        //     return _uiNode.GetViewport().GetMousePosition();
        // }

        // private Entity GetEntityUnderMouse(Vector2 mousePos)
        // {
        //     var spaceState = _uiNode.GetWorld3D().DirectSpaceState;
        //     var from = _camera.ProjectRayOrigin(mousePos);
        //     var to = from + _camera.ProjectRayNormal(mousePos) * 1000;

        //     var query = PhysicsRayQueryParameters3D.Create(from, to);
        //     query.CollideWithAreas = true;
        //     query.CollideWithBodies = true;

        //     var result = spaceState.IntersectRay(query);

        //     if (result.Count > 0 && result.ContainsKey("collider"))
        //     {
        //         var collider = result["collider"].As<Node3D>();
        //         if (collider != null)
        //         {
        //             var entityId = int.TryParse(collider.Name, out var id) ? id : -1;
        //             return Entities.Query<Unit>().FirstOrDefault(e => e.Id == entityId);
        //         }
        //     }

        //     return null;
        // }

        // public void RemoveHeart()
        // {
        //     if (_hearts.Count == 0)
        //         return;

        //     var heart = _hearts.Pop();
        //     _uiNode.RemoveChild(heart);
        //     heart.QueueFree();
        // }

        // public void AddHeart(float offsetLeft = 0f, float offsetRight = 0f, float offsetBottom = 40f, float offsetTop = 0f, int size = HEART_SIZE)
        // {
        //     var heart = new ColorRect
        //     {
        //         OffsetLeft = offsetLeft,
        //         OffsetRight = offsetRight,
        //         OffsetBottom = offsetBottom,
        //         Color = new Color(1, 0.0862745f, 0.321569f, 1),
        //         Size = new Vector2(size, size)
        //     };
        //     _hearts.Push(heart);
        //     _uiNode.AddChild(heart);
        // }

        // public override void Cleanup()
        // {
        //     Input.MouseButtonPressed -= OnMouseButtonPressed;

        //     foreach (var heart in _hearts)
        //     {
        //         heart.QueueFree();
        //     }
        //     _hearts.Clear();
        // }
    }
}