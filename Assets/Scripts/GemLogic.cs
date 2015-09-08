using System.Linq;
using UnityEngine;

namespace Assets.Scripts {
    public class GemLogic: MonoBehaviour {
        public Constants.BonusType Bonus;
        public Sprite BonusCollumnBombSprite;
        public Sprite BonusRowBombSprite;
        public Sprite[] GemTypes;
        public Vector2 GridPos;
        private Camera _camera;

        private int _curType = -1;
        private SpriteRenderer _spriteRenderer;

        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _camera = Camera.main;

            if (_curType == -1){
                _curType = Random.Range(0, GemTypes.Length);
            }
            SetType(_curType);
        }

        public void Init(int x, int y, Constants.BonusType bonus) {
            GridPos = new Vector2(x, y);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (bonus != Constants.BonusType.NoBonus){
                AddBonus(bonus);
            }
        }

        private void Update() {
            var goalPos = Utils.GetScreenPosByGrid(GridPos);
            goalPos = _camera.ScreenToWorldPoint(goalPos);
            transform.position = Vector2.Lerp(goalPos, transform.position, Constants.GemTransitionTime*2);
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
            if (type >= 0 && type < GemTypes.Length){
                _curType = type;
                _spriteRenderer.sprite = GemTypes[type];
            }
        }

        public int GetGemType() {
            return _curType;
        }

        public void AddBonus(Constants.BonusType bonus) {
            Bonus = bonus;
            var bonusGO = new GameObject();
            bonusGO.transform.parent = transform;
            bonusGO.AddComponent<SpriteRenderer>();
            bonusGO.transform.localPosition = new Vector3(0, 0, -1);

            if (bonus == Constants.BonusType.CollumnBomb){
                bonusGO.name = "CollumnBonus";
                bonusGO.GetComponent<SpriteRenderer>().sprite = BonusCollumnBombSprite;
            }
            if (bonus == Constants.BonusType.RowBomb){
                bonusGO.name = "RowBonus";
                bonusGO.GetComponent<SpriteRenderer>().sprite = BonusRowBombSprite;
            }
        }

        public void BonusAction(GameManager gManager) {
            if (Bonus == Constants.BonusType.CollumnBomb){
                var toDestroy = gManager.Gems.Where(g=>GridPos.x == g.GridPos.x).ToList();
                foreach (var gem in toDestroy){
                    gManager.DestroyGem(gem.GridPos);
                }
            }
            if (Bonus == Constants.BonusType.RowBomb){
                var toDestroy = gManager.Gems.Where(g=>GridPos.y == g.GridPos.y).ToList();
                foreach (var gem in toDestroy){
                    gManager.DestroyGem(gem.GridPos);
                }
            }
        }
    }
}