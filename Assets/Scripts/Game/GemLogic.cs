using System.Linq;
using Assets.Scripts.Game;
using UnityEngine;

namespace Assets.Scripts {
    public class GemLogic: MonoBehaviour {
        public Constants.BonusType Bonus;
        public Sprite BonusCollumnBombSprite;
        public Sprite BonusRowBombSprite;

        public Vector2 GridPos;

        private BoardLogic _board;
        private int _curType = -1;
        private SpriteRenderer _spriteRenderer;

        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_curType == -1){
                _curType = Random.Range(0, _board.GemTypes.Length);
            }
            SetType(_curType);
        }

        public void Init(BoardLogic board, int x, int y, Constants.BonusType bonus) {
            _board = board;
            transform.parent = board.transform;

            GridPos = new Vector2(x, y);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (bonus != Constants.BonusType.NoBonus){
                AddBonus(bonus);
            }

            // Calc Position:
            var screenPos = Utils.GetScreenPosByGrid(new Vector2(x, y), transform);
            var goalWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
            transform.localPosition = goalWorldPos;
        }

        private void Update() {
            UpdatePos();
        }

        private void UpdatePos() {
            var goalScreenPos = Utils.GetScreenPosByGrid(GridPos, _board.transform);
            var goalWorldPos = Camera.main.ScreenToWorldPoint(goalScreenPos);

            transform.localPosition = Vector2.Lerp(goalWorldPos, transform.localPosition, Constants.GemTransitionTime*2);
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
            if (type >= 0 && type < _board.GemTypes.Length){
                _curType = type;
                _spriteRenderer.sprite = _board.GemTypes[type];
            }
        }

        public int GetGemType() {
            return _curType;
        }

        /// <summary>
        ///     Добавляет бонус-эффект к гему. Тут добавляется внешний вид
        ///     Действия описываются в BonusAction()
        /// </summary>
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

        /// <summary>
        ///     Описываются действия каждого бонус-эффекта. Вызывается при уничтожении гема
        /// </summary>
        public void BonusAction() {
            if (Bonus == Constants.BonusType.CollumnBomb){
                var toDestroy = _board.Gems.Where(g=>GridPos.x == g.GridPos.x).ToList();
                foreach (var gem in toDestroy){
                    _board.DestroyGem((int) gem.GridPos.x, (int) gem.GridPos.y);
                }
            }
            if (Bonus == Constants.BonusType.RowBomb){
                var toDestroy = _board.Gems.Where(g=>GridPos.y == g.GridPos.y).ToList();
                foreach (var gem in toDestroy){
                    _board.DestroyGem((int) gem.GridPos.x, (int) gem.GridPos.y);
                }
            }
        }
    }
}