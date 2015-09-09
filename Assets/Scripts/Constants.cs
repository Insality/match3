using UnityEngine;

namespace Assets.Scripts {
    public class Constants: MonoBehaviour {
        public enum BonusType {
            NoBonus,
            RowBomb,
            CollumnBomb
        }

        public enum GameState {
            WaitingInput,
            Animation
        }

        // Level settings:
        public const int GemSize = 64;

        // Gameplay settings:
        public const int ScorePerGem = 25;

        // Animation settings:
        public const float GemTransitionTime = 0.30f;
    }
}