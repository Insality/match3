using UnityEngine;

namespace Assets.Scripts {
    public class Constants : MonoBehaviour {
        // Level settings:
        public const int LevelWidth = 8;
        public const int LevelHeight = 10;
        public const int GemSize = 64;
        public const float GemGapSize = 16;

        // Gameplay settings:
        public const int ScorePerGem = 50;

        // Animation settings:
        public const float GemTransitionTime = 0.4f;
    }
}
