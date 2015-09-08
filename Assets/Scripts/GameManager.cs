using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts {
    public class GameManager: MonoBehaviour {
        private const int GridHeight = Constants.LevelHeight*Constants.GemSize;
        private const int GridWidth = Constants.LevelWidth*Constants.GemSize;
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

            _gridOffsetX = (Screen.width - GridWidth)/2;
            _gridOffsetY = (Screen.height - GridHeight)/2;
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

            var gemPos = GetScreenPosByGrid(new Vector2(posX, posY));

            gemPos = _camera.ScreenToWorldPoint(gemPos);
            gemPos = new Vector3(gemPos.x, gemPos.y, 0);
            gem.transform.position = gemPos;
            

            Level[posY, posX] = gem.GetComponent<GemLogic>();
        }

        private void Update() {
            UpdateControl();
        }

        private void UpdateControl() {
            if (Input.GetMouseButtonDown(0)){
                var grid = GetGridPosByScreen(Input.mousePosition);
                // If clicked on grid:
                if (grid.x >= 0 && grid.x < Constants.LevelWidth && grid.y >= 0 && grid.y < Constants.LevelHeight) {
                    ChooseGem(grid);
                }
                else{
                    _choosen = null;
                }
            }
        }

        private void ChooseGem(Vector2 gridPos) {
            var gem = GetGemByGridPos(gridPos);
            gem.SetType(Random.Range(0, 5));
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

        public void Swap(GemLogic gem1, GemLogic gem2) {
            Debug.Log("Swap gems " + gem1.PosX + " " + gem1.PosY + " And " + gem2.PosX + " " + gem2.PosY);
            SelectGem(null);
        }

        private void SelectGem(GemLogic gem) {
            _choosen = gem;
            if (gem == null){
                _choosenObject.transform.position = new Vector2(-10, -10);
            }
            else{
                var screenPos = GetScreenPosByGrid(new Vector2(gem.PosX, gem.PosY));
                var newPos = _camera.ScreenToWorldPoint(screenPos);
                newPos.z = 0;
                _choosenObject.transform.position = newPos;
            }   
        }

        public Vector2 GetGridPosByScreen(Vector2 screenPos) {
            var x = (int)((Input.mousePosition.x - _gridOffsetX + Constants.GemSize / 2) / Constants.GemSize);
            var y = (int)((Input.mousePosition.y - _gridOffsetY + Constants.GemSize / 2) / Constants.GemSize);
            return new Vector2(x, y);
        }

        public Vector2 GetScreenPosByGrid(Vector2 gridPos) {
            var screenPos = new Vector2(gridPos.x * Constants.GemSize, gridPos.y * Constants.GemSize);

            screenPos.x += _gridOffsetX;
            screenPos.y += _gridOffsetY;
            return screenPos;
        }

        public GemLogic GetGemByGridPos(Vector2 gridPos) {
            return Level[(int)gridPos.y, (int)gridPos.x];
        }
    }
}