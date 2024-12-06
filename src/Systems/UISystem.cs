using Godot;

namespace Game.Systems
{
    public class UISystem
    {
        private readonly EntityManager _entityManager;
        private Control _uiNode;

        private const int HEART_SIZE = 40;

        public UISystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _uiNode = _entityManager.GetRootNode().GetNode<Control>("UI/SubViewportContainer/SubViewport/Control");
            AddHeart();
            AddHeart(HEART_SIZE + 20);
            AddHeart(HEART_SIZE * 3);
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
            _uiNode.AddChild(heart);
        }

    }
}
