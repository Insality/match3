using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Game {
    public class BoardLogic: MonoBehaviour {
        public GameObject GemPrefab;
        public Sprite[] GemTypes;
        public List<GemLogic> Gems;

        private int _boardHeight;
        private int _boardWidth;
        private GameManager _gameManager;

        private GemLogic _lastMovedGem1;
        private GemLogic _lastMovedGem2;

        public GemLogic GetGem(int x, int y) {
            GemLogic gem = null;
            if (IsFreeTile(x, y)){
                gem = Gems.FirstOrDefault(g=>g.GridPos.x == x && g.GridPos.y == y);
            }

            return gem;
        }

        public GemLogic AddGem(int x, int y) {
            return AddGem(x, y, Constants.BonusType.NoBonus);
        }

        public GemLogic AddGem(int x, int y, Constants.BonusType bonus) {
            var gem = Instantiate(GemPrefab);
            var gemLogic = gem.GetComponent<GemLogic>();
            gemLogic.Init(this, x, y, bonus);

            Gems.Add(gemLogic);
            return gemLogic;
        }

        public void DestroyGem(int x, int y) {
            var gem = GetGem(x, y);
            if (gem != null){
                Gems.Remove(gem);
                gem.BonusAction();

                Destroy(gem.gameObject);
                _gameManager.AddScore(Constants.ScorePerGem);
            }
        }

        /// <summary>
        ///     Отвечает за перемещение гемов под силой гравитации, проверяет совпадения на текущем поле
        ///     Возвращает true, если доска была изменена за время обновления
        /// </summary>
        public bool UpdateBoard() {
            var isUpdated = false;

            for (var i = 1; i < Constants.LevelHeight; i++){
                for (var j = 0; j < Constants.LevelWidth; j++){
                    var gem = GetGem(j, i);
                    if (gem != null){
                        if (MoveGem(gem, j, i - 1)){
                            isUpdated = true;
                        }
                    }
                }
            }

            if (!isUpdated){
                DestroyMatches();
            }
            else{
                _gameManager.SetUpdatingCounter(Constants.GemTransitionTime);
            }

            RefillBoard();

            return isUpdated;
        }

        /// <summary>
        ///     Добавляет новые гемы сверху поля (по одному на колонну за раз)
        /// </summary>
        public void RefillBoard() {
            // Refill only the highest row
            var i = _boardHeight - 1;
            for (var j = 0; j < _boardWidth; j++){
                var gem = GetGem(j, i);
                if (gem == null){
                    var newGem = AddGem(j, i + 2, Constants.BonusType.NoBonus);
                    MoveGem(newGem, j, i);
                }
            }
        }

        public void InitBoard(GameManager gManager, int width, int height) {
            _gameManager = gManager;

            ClearBoard();

            transform.position = new Vector3(0f, 0f, 0);

            _boardHeight = height;
            _boardWidth = width;
            Gems = new List<GemLogic>(height*width);

            for (var i = 0; i < _boardWidth; i++){
                for (var j = 0; j < _boardHeight; j++){
                    AddGem(i, j);
                }
            }
        }

        public void ClearBoard() {
            foreach (var gem in Gems.ToList()){
                DestroyGem((int) gem.GridPos.x, (int) gem.GridPos.y);
            }
            Gems.Clear();
        }

        /// <summary>
        ///     Пытается подвинуть гем на пустое место. Если клетка занята - вернет false
        /// </summary>
        public bool MoveGem(GemLogic gem, int x, int y) {
            var isMoved = false;
            if (GetGem(x, y) == null && IsFreeTile(x, y)){
                gem.GridPos = new Vector2(x, y);
                isMoved = true;
            }
            return isMoved;
        }

        /// <summary>
        ///     Возвращает true, если клетка (x, y) может быть занята или уже занята гемом
        /// </summary>
        public bool IsFreeTile(int x, int y) {
            return (x >= 0 && x < _boardWidth && y >= 0 && y < _boardHeight);
        }

        public void SwapGems(GemLogic gem1, GemLogic gem2) {
            Debug.Log("Swapping");
            var tmpPos = gem1.GridPos;
            gem1.GridPos = gem2.GridPos;
            gem2.GridPos = tmpPos;

            _lastMovedGem1 = gem1;
            _lastMovedGem2 = gem2;
            Invoke("CheckReverseTurn", Constants.GemTransitionTime/2);

            _gameManager.SetUpdatingCounter(Constants.GemTransitionTime);
        }

        public void CheckReverseTurn() {
            if (!GetMatches().Any()){
                Debug.Log("Reverse turn");
                var tmpPos = _lastMovedGem1.GridPos;
                _lastMovedGem1.GridPos = _lastMovedGem2.GridPos;
                _lastMovedGem2.GridPos = tmpPos;
            }
        }


        /// <summary>
        ///     Проверяет все совпадения на текущей доске.
        ///     Каждый список - список гемов, которые должны исчезнуть
        /// </summary>
        public List<List<GemLogic>> GetMatches() {
            var matches = new List<List<GemLogic>>();
            // check rows:
            for (var i = 0; i < _boardHeight; i++){
                var lastMatch = new List<GemLogic>();
                for (var j = 0; j < _boardWidth; j++){
                    var gem = GetGem(j, i);
                    if (gem != null){
                        if (!lastMatch.Any()){
                            lastMatch.Add(gem);
                        }
                        else if (lastMatch.Last().GetGemType() == gem.GetGemType()){
                            lastMatch.Add(gem);
                        }
                        else{
                            if (lastMatch.Count >= 3){
                                // Copy list hack?
                                matches.Add(lastMatch.ToArray().ToList());
                            }
                            lastMatch.Clear();
                            lastMatch.Add(gem);
                        }
                    }
                }

                if (lastMatch.Count >= 3){
                    matches.Add(lastMatch.ToArray().ToList());
                }
                lastMatch.Clear();
            }

            // Check collumns
            for (var j = 0; j < _boardWidth; j++){
                var lastMatch = new List<GemLogic>();
                for (var i = 0; i < _boardHeight; i++){
                    var gem = GetGem(j, i);
                    if (gem != null){
                        if (!lastMatch.Any()){
                            lastMatch.Add(gem);
                        }
                        else if (lastMatch.Last().GetGemType() == gem.GetGemType()){
                            lastMatch.Add(gem);
                        }
                        else{
                            if (lastMatch.Count >= 3){
                                matches.Add(lastMatch.ToArray().ToList());
                            }
                            lastMatch.Clear();
                            lastMatch.Add(gem);
                        }
                    }
                }

                if (lastMatch.Count >= 3){
                    matches.Add(lastMatch.ToArray().ToList());
                }
                lastMatch.Clear();
            }

            return matches;
        }

        public void DestroyMatches() {
            var matches = GetMatches();
            foreach (var match in matches){
                var centerPos = match[match.Count/2].GridPos;
                var type = match[0].GetGemType();
                var bonusType = Constants.BonusType.NoBonus;
                if (match[0].GridPos.x == match[1].GridPos.x){
                    bonusType = Constants.BonusType.CollumnBomb;
                }
                else if (match[0].GridPos.y == match[1].GridPos.y){
                    bonusType = Constants.BonusType.RowBomb;
                }

                foreach (var gemLogic in match){
                    DestroyGem((int) gemLogic.GridPos.x, (int) gemLogic.GridPos.y);
                }

                if (match.Count > 3){
                    var gem = AddGem((int) centerPos.x, (int) centerPos.y, bonusType);
                    gem.SetType(type);
                }
            }
        }
    }
}