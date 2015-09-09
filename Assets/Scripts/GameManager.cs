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
            // Block mouse control if grid on updating
            if (_gameState == Constants.GameState.WaitingInput){
                // Simple click
                if (Input.GetMouseButtonDown(0)){
                    var gridPos = Utils.GetGridPosByScreenPos(Input.mousePosition, BoardLogic.transform);
                    // If clicked on grid:
                    var gem = BoardLogic.GetGem(gridPos);
                    if (gem != null && _choosenGem != null && _choosenGem.IsCanSwap(gridPos)){
                        BoardLogic.SwapGems(_choosenGem, BoardLogic.GetGem(gridPos));
                        SetActiveGem(null);
                    }
                    else{
                        SetActiveGem(gem);
                    }
                }

                if (Input.GetMouseButtonDown(1)){
                    SetActiveGem(null);
                }

                if (Input.GetMouseButtonDown(2)){
                    var gridPos = Utils.GetGridPosByScreenPos(Input.mousePosition, BoardLogic.transform);
                    BoardLogic.DestroyGem(gridPos);
                }

                // Mouse drag control:
                if (_choosenGem != null && Input.GetMouseButton(0)){
                    var grid = Utils.GetGridPosByScreenPos(Input.mousePosition, BoardLogic.transform);
                    var gem = BoardLogic.GetGem(grid);
                    if (gem != null && _choosenGem.IsCanSwap(grid)){
                        BoardLogic.SwapGems(_choosenGem, BoardLogic.GetGem((int) grid.x, (int) grid.y));
                        SetActiveGem(null);
                    }
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
                var screenPos = Utils.GetScreenPosByGridPos(gem.GetVectorPos(), BoardLogic.transform);
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