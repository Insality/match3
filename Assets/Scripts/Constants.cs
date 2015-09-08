using UnityEngine;

namespace Assets.Scripts {
    public class Constants: MonoBehaviour {
        
        public enum BonusType {
            NoBonus,
            RowBomb,
            CollumnBomb
        }

        // Level settings:
        public const int LevelWidth = 8;
        public const int LevelHeight = 10;
        public const int GemSize = 64;

        public const int GridHeight = LevelHeight*GemSize;
        public const int GridWidth = LevelWidth*GemSize;

        // Gameplay settings:
        public const int ScorePerGem = 25;

        // Animation settings:
        public const float GemTransitionTime = 0.35f;

    }
}