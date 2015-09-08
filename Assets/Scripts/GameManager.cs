using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts {
    public class GameManager: MonoBehaviour {
        public Sprite CursorSprite;

        public GameObject GemPrefab;
        public GemLogic[,] Level;

        private Camera _camera;
        private GemLogic _choosen;
        private GameObject _choosenObject;

        private int _gridOffsetX;
        private int _gridOffsetY;

        private void Start() {
            _camera = Camera.main;
            _choosenObject = new GameObject {name = "Cursor"};
            _choosenObject.AddComponent<SpriteRenderer>();
            _choosenObject.GetComponent<SpriteRenderer>().sprite = CursorSprite;


            Level = new GemLogic[Constants.LevelHeight, Constants.LevelWidth];

            InitLevel();
        }

        private void InitLevel() {
            SelectGem(null);
            for (var i = 0; i < Constants.LevelHeight; i++){
                for (var j = 0; j < Constants.LevelWidth; j++){
                    CreateGem(j, i);
                }
            }
        }

        private void CreateGem(int posX, int posY) {
            var gem = Instantiate(GemPrefab);
            gem.GetComponent<GemLogic>().Init(posX, posY);

            var gemPos = Utils.GetScreenPosByGrid(new Vector2(posX, posY));

            gemPos = _camera.ScreenToWorldPoint(gemPos);
            gemPos = new Vector3(gemPos.x, gemPos.y, 0);
            gem.transform.position = gemPos;


            Level[posY, posX] = gem.GetComponent<GemLogic>();
        }

        private void Update() {
            UpdateControl();

            if (Input.GetKeyDown(KeyCode.Space)){
                var matches = GetMatchesGem();
                foreach (var match in matches){
                    foreach (var gemLogic in match){
                        DestroyGem(gemLogic.GridPos);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.U)){
                UpdateGrid();
            }
            if (Input.GetKeyDown(KeyCode.R)){
                RefillGrid();
            }
        }

        private void UpdateControl() {
            if (Input.GetMouseButtonDown(0)){
                var grid = Utils.GetGridPosByScreen(Input.mousePosition);
                // If clicked on grid:
                if (grid.x >= 0 && grid.x < Constants.LevelWidth && grid.y >= 0 && grid.y < Constants.LevelHeight){
                    ChooseGem(grid);
                }
                else{
                    _choosen = null;
                }
            }
        }

        private void UpdateGrid() {
            for (var i = 1; i < Constants.LevelHeight; i++){
                for (var j = 1; j < Constants.LevelWidth; j++){
                    if (Level[i - 1, j] == null){
                        MoveGem(Level[i, j], new Vector2(j, i - 1));
                        Level[i, j] = null;
                    }
                }
            }
        }

        private void RefillGrid() {
            for (var i = 0; i < Constants.LevelHeight; i++){
                for (var j = 0; j < Constants.LevelWidth; j++){
                    if (Level[i, j] == null){
                        CreateGem(j, i);
                    }
                }
            }
        }

        private void ChooseGem(Vector2 gridPos) {
            var gem = GetGemByGridPos(gridPos);
            if (_choosen != null){
                if (_choosen.IsCanSwap(gridPos)){
                    Swap(_choosen, GetGemByGridPos(gridPos));
                }
                else{
                    SelectGem(gem);
                }
            }
            else{
                SelectGem(gem);
            }
        }

        public void MoveGem(GemLogic gem, Vector2 GridPos) {
            gem.GridPos = GridPos;
            Level[(int) gem.GridPos.y, (int) gem.GridPos.x] = gem;
        }

        public void DestroyGem(Vector2 gridPos) {
            var gem = GetGemByGridPos(gridPos);
            Destroy(gem.gameObject);
            Level[(int) gridPos.y, (int) gridPos.x] = null;
        }

        public void Swap(GemLogic gem1, GemLogic gem2) {
            var tmpPos = gem1.GridPos;
            MoveGem(gem1, gem2.GridPos);
            MoveGem(gem2, tmpPos);

            SelectGem(null);
        }

        private void SelectGem(GemLogic gem) {
            _choosen = gem;
            if (gem == null){
                _choosenObject.transform.position = new Vector2(-10, -10);
            }
            else{
                var screenPos = Utils.GetScreenPosByGrid(gem.GridPos);
                var newPos = _camera.ScreenToWorldPoint(screenPos);
                newPos.z = 0;
                _choosenObject.transform.position = newPos;
            }
        }


        public List<List<GemLogic>> GetMatchesGem() {
            var matches = new List<List<GemLogic>>();
            // check rows:
            for (var i = 0; i < Constants.LevelHeight; i++){
                var lastMatch = new List<GemLogic>();
                for (var j = 0; j < Constants.LevelWidth; j++){
                    if (!lastMatch.Any()){
                        lastMatch.Add(Level[i, j]);
                    }
                    else if (lastMatch.Last().GetGemType() == Level[i, j].GetGemType()){
                        lastMatch.Add(Level[i, j]);
                    }
                    else {
                        if (lastMatch.Count >= 3){
                            matches.Add(lastMatch);
                        }
                        lastMatch.Clear();
                    }
                }
                lastMatch.Clear();
            }

            // Check collumns
            for (var j = 0; j < Constants.LevelWidth; j++){
                var lastMatch = new List<GemLogic>();
                for (var i = 0; i < Constants.LevelHeight; i++){
                    if (!lastMatch.Any()){
                        lastMatch.Add(Level[i, j]);
                    }
                    else if (lastMatch.Last().GetGemType() == Level[i, j].GetGemType()){
                        lastMatch.Add(Level[i, j]);
                    }
                    else{
                        if (lastMatch.Count >= 3) matches.Add(lastMatch.ToList());
                        lastMatch.Clear();
                    }
                }
                lastMatch.Clear();
            }

            return matches;
        }

        public GemLogic GetGemByGridPos(Vector2 gridPos) {
            return Level[(int) gridPos.y, (int) gridPos.x];
        }
    }
}