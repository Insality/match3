using Assets.Scripts.Game;
using UnityEngine;

namespace Assets.Scripts {
    public class GameManager: MonoBehaviour {
        public BoardLogic BoardLogic;
        public Camera Camera;
        public Sprite CursorSprite;

        private GemLogic _choosenGem;
        private GameObject _cursor;

        private PlayerStats _player;
        private float _updatingGridCounter;

        private void Start() {
            Camera = Camera.main;
            _player = GetComponent<PlayerStats>();

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

            _updatingGridCounter -= Time.deltaTime;
            if (_updatingGridCounter <= 0){
                _updatingGridCounter = 0;
                BoardLogic.UpdateBoard();
            }
        }

        public void SetUpdatingCounter(float time) {
            _updatingGridCounter = time;
        }

        private void UpdateControl() {
            // Block control if grid on updating
            // TODO: Change to game-states.
            if (_updatingGridCounter <= 0){
                if (Input.GetMouseButtonDown(0)){
                    var grid = Utils.GetGridPosByScreen(Input.mousePosition, BoardLogic.transform);
                    // If clicked on grid:
                    var gem = BoardLogic.GetGem((int) grid.x, (int) grid.y);
                    if (gem != null){
                        if (_choosenGem != null){
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
                    else{
                        SetActiveGem(null);
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
                var screenPos = Utils.GetScreenPosByGrid(gem.GridPos, BoardLogic.transform);
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