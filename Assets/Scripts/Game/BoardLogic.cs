using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Game {
    public class BoardLogic: MonoBehaviour {
        public int BoardHeight;
        public int BoardWidth;
        public GameObject GemPrefab;
        public Sprite[] GemTypes;
        public List<GemLogic> Gems;

        private GameManager _gameManager;

        private GemLogic _lastMovedGem1;
        private GemLogic _lastMovedGem2;

        public GemLogic GetGem(int x, int y) {
            GemLogic gem = null;
            if (IsFreeTile(x, y)){
                gem = Gems.FirstOrDefault(g=>g.X == x && g.Y == y);
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
        /// </summary>
        public void UpdateBoard() {
            if (!UpdateGravity()){
                DestroyMatches();
            }
            else{
                _gameManager.SetAnimationState(Constants.GemTransitionTime);
            }
            RefillBoard();
        }

        private bool UpdateGravity() {
            var isUpdated = false;
            for (var i = 1; i < BoardHeight; i++){
                for (var j = 0; j < BoardWidth; j++){
                    var gem = GetGem(j, i);
                    if (gem != null){
                        if (MoveGem(gem, j, i - 1)){
                            isUpdated = true;
                        }
                    }
                }
            }
            return isUpdated;
        }

        /// <summary>
        ///     Добавляет новые гемы сверху поля (по одному на колонну за раз)
        /// </summary>
        public void RefillBoard() {
            // Refill only the highest row
            var i = BoardHeight - 1;
            for (var j = 0; j < BoardWidth; j++){
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

            BoardHeight = height;
            BoardWidth = width;
            Gems = new List<GemLogic>(height*width);

            for (var i = 0; i < BoardWidth; i++){
                for (var j = 0; j < BoardHeight; j++){
                    AddGem(i, j);
                }
            }
        }

        public void ClearBoard() {
            foreach (var gem in Gems.ToList()){
                DestroyGem(gem.X, gem.Y);
            }
            Gems.Clear();
        }

        /// <summary>
        ///     Пытается подвинуть гем на пустое место. Если клетка занята - вернет false
        /// </summary>
        public bool MoveGem(GemLogic gem, int x, int y) {
            var isMoved = false;
            if (GetGem(x, y) == null && IsFreeTile(x, y)){
                gem.SetPos(x, y);
                isMoved = true;
            }
            return isMoved;
        }

        /// <summary>
        ///     Возвращает true, если клетка (x, y) может быть занята или уже занята гемом
        /// </summary>
        public bool IsFreeTile(int x, int y) {
            return (x >= 0 && x < BoardWidth && y >= 0 && y < BoardHeight);
        }

        public void SwapGems(GemLogic gem1, GemLogic gem2) {
            Debug.Log("Swapping");
            var tmpPos = gem1.GetVectorPos();
            gem1.SetPos(gem2.X, gem2.Y);
            gem2.SetPos((int) tmpPos.x, (int) tmpPos.y);

            _lastMovedGem1 = gem1;
            _lastMovedGem2 = gem2;
            Invoke("CheckReverseTurn", Constants.GemTransitionTime/2);

            _gameManager.SetAnimationState(Constants.GemTransitionTime);
        }

        public void CheckReverseTurn() {
            if (!GetMatches().Any()){
                Debug.Log("Reverse turn");
                var tmpPos = _lastMovedGem1.GetVectorPos();
                _lastMovedGem1.SetPos(_lastMovedGem2.X, _lastMovedGem2.Y);
                _lastMovedGem2.SetPos((int) tmpPos.x, (int) tmpPos.y);
            }
        }


        /// <summary>
        ///     Проверяет все совпадения на текущей доске.
        ///     Каждый список - список гемов, которые должны исчезнуть
        /// </summary>
        public List<List<GemLogic>> GetMatches() {
            var matches = new List<List<GemLogic>>();
            // check rows:
            for (var i = 0; i < BoardHeight; i++){
                var lastMatch = new List<GemLogic>();
                for (var j = 0; j < BoardWidth; j++){
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
            for (var j = 0; j < BoardWidth; j++){
                var lastMatch = new List<GemLogic>();
                for (var i = 0; i < BoardHeight; i++){
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
                var centerPos = match[match.Count/2].GetVectorPos();
                var type = match[0].GetGemType();
                var bonusType = Constants.BonusType.NoBonus;
                if (match[0].X == match[1].X){
                    bonusType = Constants.BonusType.CollumnBomb;
                }
                else if (match[0].Y == match[1].Y){
                    bonusType = Constants.BonusType.RowBomb;
                }

                foreach (var gemLogic in match){
                    DestroyGem(gemLogic.X, gemLogic.Y);
                }

                if (match.Count > 3){
                    var gem = AddGem((int) centerPos.x, (int) centerPos.y, bonusType);
                    gem.SetType(type);
                }
            }
        }
    }
}