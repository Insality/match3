using UnityEngine;

namespace Assets.Scripts {
    public class Utils: MonoBehaviour {
        public static Vector2 GetGridPosByScreen(Vector2 screenPos) {
            var x = (int)((Input.mousePosition.x - Constants.GridOffsetX + Constants.GemSize / 2) / Constants.GemSize);
            var y = (int)((Input.mousePosition.y - Constants.GridOffsetY + Constants.GemSize / 2) / Constants.GemSize);
            return new Vector2(x, y);
        }

        public static Vector2 GetScreenPosByGrid(Vector2 gridPos) {
            var screenPos = new Vector2(gridPos.x * Constants.GemSize, gridPos.y * Constants.GemSize);

            screenPos.x += Constants.GridOffsetX;
            screenPos.y += Constants.GridOffsetY;
            return screenPos;
        }
    }
}