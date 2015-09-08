using UnityEngine;

namespace Assets.Scripts {
    public class GemLogic: MonoBehaviour {
        public Sprite[] GemTypes;
        public Vector2 GridPos;
        private BoxCollider2D _boxCollider;
        private Camera _camera;

        private int _curType;
        private SpriteRenderer _spriteRenderer;

        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _camera = Camera.main;

            SetType(Random.Range(0, GemTypes.Length));
        }

        public void Init(int x, int y) {
            GridPos = new Vector2(x, y);
        }

        private void Update() {
            var goal = Utils.GetScreenPosByGrid(GridPos);
            goal = _camera.ScreenToWorldPoint(goal);
            transform.position = Vector2.Lerp(goal, transform.position, Constants.GemTransitionTime);
        }

        public bool IsCanSwap(Vector2 pos) {
            if (pos.x >= 0 && pos.x < Constants.LevelWidth && pos.y >= 0 && pos.y < Constants.LevelHeight){
                // Chech if pos is Neighbor
                if (Mathf.Abs(GridPos.x - pos.x) == 1 && (GridPos.y - pos.y == 0) ||
                    Mathf.Abs(GridPos.y - pos.y) == 1 && (GridPos.x - pos.x == 0)){
                    return true;
                }
                return false;
            }
            return false;
        }

        public void SetType(int type) {
            if (type <= GemTypes.Length){
                _curType = type;
                _spriteRenderer.sprite = GemTypes[type];
            }
        }

        public int GetGemType() {
            return _curType;
        }
    }
}