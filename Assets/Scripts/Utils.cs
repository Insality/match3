using UnityEngine;

namespace Assets.Scripts {
    public class Utils: MonoBehaviour {
        public static Vector2 GetGridPosByScreenPos(Vector2 screenPos, Transform boardTransform) {
            var boardScreen = Camera.main.WorldToScreenPoint(boardTransform.position);

            var x = (int) ((Input.mousePosition.x - boardScreen.x + Constants.GemSize/2)/Constants.GemSize);
            var y = (int) ((Input.mousePosition.y - boardScreen.y + Constants.GemSize/2)/Constants.GemSize);
            return new Vector2(x, y);
        }

        public static Vector2 GetScreenPosByGridPos(Vector2 gridPos, Transform boardTransform) {
            var boardScreen = Camera.main.WorldToScreenPoint(boardTransform.position);

            var screenPos = new Vector2(gridPos.x*Constants.GemSize, gridPos.y*Constants.GemSize);

            screenPos.x += boardScreen.x;
            screenPos.y += boardScreen.y;
            return screenPos;
        }
    }
}