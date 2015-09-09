using Assets.Scripts.Game;
using UnityEngine;

namespace Assets.Scripts {
    public class GameManager: MonoBehaviour {
        public BoardLogic BoardLogic;
        public Camera Camera;
        public Sprite CursorSprite;

        private GemLogic _choosenGem;
        private GameObject _cursor;
        private Constants.GameState _gameState;

        private PlayerStats _player;
        private float _stateAnimationCounter;

        private void Start() {
            Camera = Camera.main;
            _player = GetComponent<PlayerStats>();
            _gameState = Constants.GameState.WaitingInput;

            _cursor = new GameObject {name = "Cursor"};
            _cursor.AddComponent<SpriteRenderer>();
            _cursor.GetComponent<SpriteRenderer>().sprite = CursorSprite;
            SetActiveGem(null);

            // New
            BoardLogic = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardLogic>();
            BoardLogic.InitBoard(this, 6, 8);
        }


        private void Update() {
            UpdateControl();
            UpdateGameState();

            if (_gameState == Constants.GameState.WaitingInput){
                BoardLogic.UpdateBoard();
            }
        }

        public void SetAnimationState(float time) {
            _stateAnimationCounter = time;
            _gameState = Constants.GameState.Animation;
        }

        private void UpdateGameState() {
            switch (_gameState){
                case Constants.GameState.WaitingInput:
                    break;
                case Constants.GameState.Animation:
                    _stateAnimationCounter -= Time.deltaTime;
                    if (_stateAnimationCounter <= 0){
                        _stateAnimationCounter = 0;
                        _gameState = Constants.GameState.WaitingInput;
                    }
                    break;
            }
        }

        private void UpdateControl() {
            // Block control if grid on updating
            if (_gameState == Constants.GameState.WaitingInput){
                if (Input.GetMouseButtonDown(0)){
                    var grid = Utils.GetGridPosByScreen(Input.mousePosition, BoardLogic.transform);
                    // If clicked on grid:
                    var gem = BoardLogic.GetGem((int) grid.x, (int) grid.y);
                    if (gem != null && _choosenGem != null){
                        if (_choosenGem.IsCanSwap(grid)){
                            BoardLogic.SwapGems(_choosenGem, BoardLogic.GetGem((int) grid.x, (int) grid.y));
                            SetActiveGem(null);
                        }
                        else{
                            SetActiveGem(gem);
                        }
                    }
                    else{
                        SetActiveGem(gem);
                    }
                }

                if (Input.GetMouseButtonDown(1)){
                    var grid = Utils.GetGridPosByScreen(Input.mousePosition, BoardLogic.transform);
                    BoardLogic.DestroyGem((int) grid.x, (int) grid.y);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space)){
                BoardLogic.ClearBoard();
            }
        }

        private void SetActiveGem(GemLogic gem) {
            _choosenGem = gem;
            if (gem == null){
                _cursor.transform.position = new Vector3(-10, -10);
            }
            else{
                var screenPos = Utils.GetScreenPosByGrid(gem.GetVectorPos(), BoardLogic.transform);
                var newPos = Camera.ScreenToWorldPoint(screenPos);
                newPos.z = -5;
                _cursor.transform.position = newPos;
            }
        }

        public void AddScore(int score) {
            _player.AddScore(score);
        }
    }
}