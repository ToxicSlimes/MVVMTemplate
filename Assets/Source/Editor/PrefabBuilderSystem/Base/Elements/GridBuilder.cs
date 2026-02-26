#if UNITY_EDITOR
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Elements
{
    public class GridBuilder : BaseElementBuilder<GridBuilder, GridLayoutGroup>
    {
        protected override string NamingPrefix => NamingConvention.Grid;
        
        private Vector2 _cellSize = new(100f, 100f);
        private Vector2 _spacing = new(0f, 0f);
        private RectOffset _padding = new();
        private GridLayoutGroup.Corner _startCorner = GridLayoutGroup.Corner.UpperLeft;
        private GridLayoutGroup.Axis _startAxis = GridLayoutGroup.Axis.Horizontal;
        private TextAnchor _childAlignment = TextAnchor.UpperLeft;
        private GridLayoutGroup.Constraint _constraint = GridLayoutGroup.Constraint.Flexible;
        private int _constraintCount = 2;
        private Color? _backgroundColor;
        private Sprite _backgroundSprite;

        public GridBuilder(string name) : base(name) { }

        public GridBuilder WithCellSize(float width, float height)
        {
            _cellSize = new Vector2(width, height);
            return this;
        }

        public GridBuilder WithCellSize(Vector2 size)
        {
            _cellSize = size;
            return this;
        }

        public GridBuilder WithSpacing(float x, float y)
        {
            _spacing = new Vector2(x, y);
            return this;
        }

        public GridBuilder WithSpacing(Vector2 spacing)
        {
            _spacing = spacing;
            return this;
        }

        public GridBuilder WithPadding(int all)
        {
            _padding = new RectOffset(all, all, all, all);
            return this;
        }

        public GridBuilder WithPadding(int left, int right, int top, int bottom)
        {
            _padding = new RectOffset(left, right, top, bottom);
            return this;
        }

        public GridBuilder FromTopLeft()
        {
            _startCorner = GridLayoutGroup.Corner.UpperLeft;
            return this;
        }

        public GridBuilder FromTopRight()
        {
            _startCorner = GridLayoutGroup.Corner.UpperRight;
            return this;
        }

        public GridBuilder FromBottomLeft()
        {
            _startCorner = GridLayoutGroup.Corner.LowerLeft;
            return this;
        }

        public GridBuilder FromBottomRight()
        {
            _startCorner = GridLayoutGroup.Corner.LowerRight;
            return this;
        }

        public GridBuilder HorizontalFirst()
        {
            _startAxis = GridLayoutGroup.Axis.Horizontal;
            return this;
        }

        public GridBuilder VerticalFirst()
        {
            _startAxis = GridLayoutGroup.Axis.Vertical;
            return this;
        }

        public GridBuilder WithAlignment(TextAnchor alignment)
        {
            _childAlignment = alignment;
            return this;
        }

        public GridBuilder FixedColumnCount(int count)
        {
            _constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _constraintCount = count;
            return this;
        }

        public GridBuilder FixedRowCount(int count)
        {
            _constraint = GridLayoutGroup.Constraint.FixedRowCount;
            _constraintCount = count;
            return this;
        }

        public GridBuilder Flexible()
        {
            _constraint = GridLayoutGroup.Constraint.Flexible;
            return this;
        }

        public GridBuilder WithBackground(Color color)
        {
            _backgroundColor = color;
            return this;
        }

        public GridBuilder WithBackgroundSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
            return this;
        }

        protected override string GetDefaultPrefabPath() => ElementPaths.Containers.GridLayout;

        public override GridLayoutGroup Build(Transform parent)
        {
            if (UsePrefabSource)
                return BuildFromPrefab(parent);
            
            var rt = CreateRectTransform(parent);
            var go = rt.gameObject;

            if (_backgroundColor.HasValue || _backgroundSprite != null)
            {
                var img = go.AddComponent<Image>();
                img.color = _backgroundColor ?? Color.white;
                img.sprite = _backgroundSprite;
                if (_backgroundSprite != null)
                {
                    img.type = Image.Type.Sliced;
                }
            }

            var grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = _cellSize;
            grid.spacing = _spacing;
            grid.padding = _padding;
            grid.startCorner = _startCorner;
            grid.startAxis = _startAxis;
            grid.childAlignment = _childAlignment;
            grid.constraint = _constraint;
            grid.constraintCount = _constraintCount;

            ApplyLayoutElement(go);

            return grid;
        }
    }
}
#endif
