﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts {
    public class GameManager: MonoBehaviour {
        public Sprite CursorSprite;

        public GameObject GemPrefab;
        public List<GemLogic> Gems;

        private Camera _camera;
        private GemLogic _choosenGem;
        private GameObject _cursor;

        private GemLogic _lastMovedGem1;
        private GemLogic _lastMovedGem2;
        private PlayerStats _player;
        private float _updatingGridCounter;


        private void Start() {
            _camera = Camera.main;
            _player = GetComponent<PlayerStats>();

            _cursor = new GameObject {name = "Cursor"};
            _cursor.AddComponent<SpriteRenderer>();
            _cursor.GetComponent<SpriteRenderer>().sprite = CursorSprite;

            InitLevel();
        }

        private void InitLevel() {
            foreach (var gem in Gems){
                Destroy(gem.gameObject);
            }
            Gems.Clear();
            _player.Restart();

            SetActiveGem(null);
            _updatingGridCounter = 0;

            for (var i = 0; i < Constants.LevelHeight; i++){
                for (var j = 0; j < Constants.LevelWidth; j++){
                    CreateGem(j, i, Constants.BonusType.NoBonus);
                }
            }
        }

        private GemLogic CreateGem(int posX, int posY, Constants.BonusType bonus) {
            var gem = Instantiate(GemPrefab);
            var gemLogic = gem.GetComponent<GemLogic>();
            gemLogic.Init(posX, posY, bonus);

            var gemPos = Utils.GetScreenPosByGrid(new Vector2(posX, posY));
            gemPos = _camera.ScreenToWorldPoint(gemPos);
            gemPos = new Vector3(gemPos.x, gemPos.y, 0);
            gem.transform.position = gemPos;

            Gems.Add(gemLogic);
            return gemLogic;
        }

        private void Update() {
            UpdateControl();

            _updatingGridCounter -= Time.deltaTime;
            if (_updatingGridCounter <= 0){
                _updatingGridCounter = 0;
                UpdateGrid();
            }
        }


        private void UpdateControl() {
            // Block control if grid on updating
            if (_updatingGridCounter <= 0){
                if (Input.GetMouseButtonDown(0)){
                    var grid = Utils.GetGridPosByScreen(Input.mousePosition);
                    // If clicked on grid:
                    if (grid.x >= 0 && grid.x < Constants.LevelWidth && grid.y >= 0 && grid.y < Constants.LevelHeight){
                        SelectActionGem(grid);
                    }
                    else{
                        _choosenGem = null;
                    }
                }

                if (Input.GetMouseButtonDown(1)){
                    var grid = Utils.GetGridPosByScreen(Input.mousePosition);
                    DestroyGem(grid);
                }
            }


            if (Input.GetKeyDown(KeyCode.Space)){
                foreach (var gem in Gems.ToList()){
                    DestroyGem(gem.GridPos);
                }
            }
        }

        private void UpdateGrid() {
            var isUpdated = false;
            for (var i = 1; i < Constants.LevelHeight; i++){
                for (var j = 0; j < Constants.LevelWidth; j++){
                    var gem = GetGemByGridPos(new Vector2(j, i));
                    if (gem != null){
                        if (GetGemByGridPos(new Vector2(j, i - 1)) == null){
                            MoveGem(gem, new Vector2(j, i - 1));
                            isUpdated = true;
                        }
                    }
                }
            }

            if (isUpdated){
                _updatingGridCounter = Constants.GemTransitionTime;
            }
            else{
                CheckAndDestroyMatches();
            }

            RefillGrid();
        }

        private void RefillGrid() {
            const int i = Constants.LevelHeight - 1;
            for (var j = 0; j < Constants.LevelWidth; j++){
                var gem = GetGemByGridPos(new Vector2(j, i));
                if (gem == null){
                    var newGem = CreateGem(j, i + 2, Constants.BonusType.NoBonus);
                    MoveGem(newGem, new Vector2(j, i));
                }
            }
        }

        private void CheckAndDestroyMatches() {
            var matches = GetMatchGems();
            foreach (var match in matches){
                var centerPos = match[match.Count/2].GridPos;
                var type = match[0].GetGemType();
                var bonusType = Constants.BonusType.NoBonus;
                if (match[0].GridPos.x == match[1].GridPos.x){
                    bonusType = Constants.BonusType.CollumnBomb;
                }
                else if (match[0].GridPos.y == match[1].GridPos.y){
                    bonusType = Constants.BonusType.RowBomb;
                }

                foreach (var gemLogic in match){
                    DestroyGem(gemLogic.GridPos);
                }
                if (match.Count > 3){
                    var gem = CreateGem((int) centerPos.x, (int) centerPos.y, bonusType);
                    gem.SetType(type);
                }
            }
        }


        private void SelectActionGem(Vector2 gridPos) {
            var gem = GetGemByGridPos(gridPos);
            if (_choosenGem != null){
                if (_choosenGem.IsCanSwap(gridPos)){
                    SwapGems(_choosenGem, GetGemByGridPos(gridPos));
                }
                else{
                    SetActiveGem(gem);
                }
            }
            else{
                SetActiveGem(gem);
            }
        }

        private void SetActiveGem(GemLogic gem) {
            _choosenGem = gem;
            if (gem == null){
                _cursor.transform.position = new Vector3(-10, -10);
            }
            else{
                var screenPos = Utils.GetScreenPosByGrid(gem.GridPos);
                var newPos = _camera.ScreenToWorldPoint(screenPos);
                newPos.z = -5;
                _cursor.transform.position = newPos;
            }
        }

        public void MoveGem(GemLogic gem, Vector2 gridPos) {
            gem.GridPos = gridPos;
        }

        public void DestroyGem(Vector2 gridPos) {
            var gem = GetGemByGridPos(gridPos);
            if (gem != null){
                Gems.Remove(gem);
                gem.BonusAction(this);

                Destroy(gem.gameObject);
                _player.AddScore(Constants.ScorePerGem);
            }
        }

        public void SwapGems(GemLogic gem1, GemLogic gem2) {
            var tmpPos = gem1.GridPos;
            MoveGem(gem1, gem2.GridPos);
            MoveGem(gem2, tmpPos);

            _lastMovedGem1 = gem1;
            _lastMovedGem2 = gem2;
            Invoke("CheckReverseTurn", Constants.GemTransitionTime/2);

            SetActiveGem(null);
            _updatingGridCounter = Constants.GemTransitionTime;
        }

        public void CheckReverseTurn() {
            if (!GetMatchGems().Any()){
                var tmpPos = _lastMovedGem1.GridPos;
                MoveGem(_lastMovedGem1, _lastMovedGem2.GridPos);
                MoveGem(_lastMovedGem2, tmpPos);
            }
        }


        public List<List<GemLogic>> GetMatchGems() {
            var matches = new List<List<GemLogic>>();
            // check rows:
            for (var i = 0; i < Constants.LevelHeight; i++){
                var lastMatch = new List<GemLogic>();
                for (var j = 0; j < Constants.LevelWidth; j++){
                    var gem = GetGemByGridPos(new Vector2(j, i));
                    if (gem != null){
                        if (!lastMatch.Any()){
                            lastMatch.Add(gem);
                        }
                        else if (lastMatch.Last().GetGemType() == gem.GetGemType()){
                            lastMatch.Add(gem);
                        }
                        else{
                            if (lastMatch.Count >= 3){
                                // Copy list hack?
                                matches.Add(lastMatch.ToArray().ToList());
                            }
                            lastMatch.Clear();
                            lastMatch.Add(gem);
                        }
                    }
                }

                if (lastMatch.Count >= 3){
                    matches.Add(lastMatch.ToArray().ToList());
                }
                lastMatch.Clear();
            }

            // Check collumns
            for (var j = 0; j < Constants.LevelWidth; j++){
                var lastMatch = new List<GemLogic>();
                for (var i = 0; i < Constants.LevelHeight; i++){
                    var gem = GetGemByGridPos(new Vector2(j, i));
                    if (gem != null){
                        if (!lastMatch.Any()){
                            lastMatch.Add(gem);
                        }
                        else if (lastMatch.Last().GetGemType() == gem.GetGemType()){
                            lastMatch.Add(gem);
                        }
                        else{
                            if (lastMatch.Count >= 3){
                                matches.Add(lastMatch.ToArray().ToList());
                            }
                            lastMatch.Clear();
                            lastMatch.Add(gem);
                        }
                    }
                }

                if (lastMatch.Count >= 3){
                    matches.Add(lastMatch.ToArray().ToList());
                }
                lastMatch.Clear();
            }

            return matches;
        }

        public GemLogic GetGemByGridPos(Vector2 gridPos) {
            var gem = Gems.FirstOrDefault(g=>g.GridPos == gridPos);
            return gem;
        }
    }
}