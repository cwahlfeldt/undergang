using System.Collections.Generic;
using Godot;

namespace Game.Systems
{
    public class UISystem
    {
        private readonly EntityManager _entityManager;
        private Stack<Control> _hearts = [];
        private Control _uiNode;
        private const int HEART_SIZE = 32;
        private const int HEART_SPACING = 8;

        public UISystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _uiNode = _entityManager.GetRootNode().GetNode<Control>("UI/SubViewportContainer/SubViewport/Control");

            var playerHealth = _entityManager.GetPlayer().Get<HealthComponent>().Health;
            for (int i = 0; i < playerHealth; i++)
            {
                AddHeart(i * (HEART_SIZE + HEART_SPACING));
            }
        }

        public void RemoveHeart()
        {
            if (_hearts.Count == 0)
                return;

            var heart = _hearts.Pop();
            _uiNode.RemoveChild(heart);
            heart.QueueFree();
        }

        public void AddHeart(float offsetLeft = 0f, float offsetRight = 0f, float offsetBottom = 40f, float offsetTop = 0f, int size = HEART_SIZE)
        {
            var heart = new ColorRect
            {
                // Name = "Heart",
                OffsetLeft = offsetLeft,
                OffsetRight = offsetRight,
                OffsetBottom = offsetBottom,
                Color = new Color(1, 0.0862745f, 0.321569f, 1),
                Size = new Vector2(size, size)
            };
            _hearts.Push(heart);
            _uiNode.AddChild(heart);
        }
    }
}
