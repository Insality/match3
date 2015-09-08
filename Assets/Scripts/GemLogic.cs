using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts {
    public class GemLogic: MonoBehaviour {
        public Sprite[] GemTypes;
        public int PosX;
        public int PosY;

        private int _curType = 0;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;

        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();

            SetType(Random.Range(0, GemTypes.Length));
        }

        public void Init(int x, int y) {
            PosX = x;
            PosY = y;
        }

        private void Update() {
        }

        public bool IsCanSwap(Vector2 pos) {
            if (pos.x >= 0 && pos.x < Constants.LevelWidth && pos.y >= 0 && pos.y < Constants.LevelHeight){
                // Chech if pos is Neighbor
                if (Mathf.Abs(PosX - pos.x) == 1 && (PosY - pos.y == 0) ||
                    Mathf.Abs(PosY - pos.y) == 1 && (PosX - pos.x == 0)){
                    return true;
                }
                else{
                    return false;
                }
            }
            else{
                return false;
            }
        }

        public void SetType(int type) {
            if (type <= GemTypes.Length){
                _curType = type;
                _spriteRenderer.sprite = GemTypes[type];
            }
        }
    }
}