﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class GameManager: MonoBehaviour {
        public Sprite CursorSprite;

        public GameObject GemPrefab;
        public List<GemLogic> Gems;

        private Camera _camera;
        private GemLogic _choosen;
        private GameObject _choosenObject;

        private int _gridOffsetX;
        private int _gridOffsetY;

        private bool _isUpdatingGrid;
        private float _updatingGridCounter;
        private PlayerStats _player;

        private GemLogic _lastMovedGem1;
        private GemLogic _lastMovedGem2;



        private void Start() {
            _camera = Camera.main;
            _player = GetComponent<PlayerStats>();

            _choosenObject = new GameObject {name = "Cursor"};
            _choosenObject.AddComponent<SpriteRenderer>();
            _choosenObject.GetComponent<SpriteRenderer>().sprite = CursorSprite;

            InitLevel();
        }

        private void InitLevel() {
            foreach (var gem in Gems){
                Destroy(gem.gameObject);
            }
            Gems.Clear();

            _player.Restart();
            SelectGem(null);
            _isUpdatingGrid = false;
            _updatingGridCounter = 0;
            for (var i = 0; i < Constants.LevelHeight; i++){
                for (var j = 0; j < Constants.LevelWidth; j++){
                    CreateGem(j, i, Constants.BonusType.NoBonus);
                }
            }
        }

        private GemLogic CreateGem(int posX, int posY, Constants.BonusType bonus) {
            var gem = Instantiate(GemPrefab);
            gem.GetComponent<GemLogic>().Init(posX, posY, bonus);

            var gemPos = Utils.GetScreenPosByGrid(new Vector2(posX, posY));

            gemPos = _camera.ScreenToWorldPoint(gemPos);
            gemPos = new Vector3(gemPos.x, gemPos.y, 0);
            gem.transform.position = gemPos;

            Gems.Add(gem.GetComponent<GemLogic>());
            return gem.GetComponent<GemLogic>();
        }

        private void Update() {
            UpdateControl();

            _updatingGridCounter -= Time.deltaTime;
            if (_updatingGridCounter <= 0) {
                _updatingGridCounter = 0;
                UpdateGrid();
            }
        }

        private void CheckAndDestroyMathes() {
            var matches = GetMatchesGem();
            foreach (var match in matches){
                var centerPos = match[match.Count/2].GridPos;
                var type = match[0].GetGemType();
                var bonusType = Constants.BonusType.NoBonus;
                if (match[0].GridPos.x == match[1].GridPos.x){
                    bonusType = Constants.BonusType.CollumnBomb;
                } else if (match[0].GridPos.y == match[1].GridPos.y) {
                    bonusType = Constants.BonusType.RowBomb;
                }

                foreach (var gemLogic in match){
                    DestroyGem(gemLogic.GridPos);
                }
                if (match.Count > 3) {
                    var gem = CreateGem((int)centerPos.x, (int)centerPos.y, bonusType);
                    gem.SetType(type);
                }
            }
        }

        private void UpdateControl() {
            if (!_isUpdatingGrid){
                if (Input.GetMouseButtonDown(0)){
                    var grid = Utils.GetGridPosByScreen(Input.mousePosition);
                    // If clicked on grid:
                    if (grid.x >= 0 && grid.x < Constants.LevelWidth && grid.y >= 0 && grid.y < Constants.LevelHeight){
                        ChooseGem(grid);
                    }
                    else{
                        _choosen = null;
                    }
                }

                if (Input.GetMouseButtonDown(1)) {
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

        private bool UpdateGrid() {
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
                _isUpdatingGrid = true;
            }
            else{
                _isUpdatingGrid = false;
                CheckAndDestroyMathes();
            }

            RefillGrid();

            return isUpdated;
        }

        private void RefillGrid() {
            var i = Constants.LevelHeight - 1;
            for (var j = 0; j < Constants.LevelWidth; j++){
                var gem = GetGemByGridPos(new Vector2(j, i));
                if (gem == null){
                    CreateGem(j, i, Constants.BonusType.NoBonus);
                }
            }
        }

        private void ChooseGem(Vector2 gridPos) {
            var gem = GetGemByGridPos(gridPos);
            if (_choosen != null){
                if (_choosen.IsCanSwap(gridPos)){
                    Swap(_choosen, GetGemByGridPos(gridPos));
                }
                else{
                    SelectGem(gem);
                }
            }
            else{
                SelectGem(gem);
            }
        }

        public void MoveGem(GemLogic gem, Vector2 GridPos) {
            gem.GridPos = GridPos;
        }

        public void DestroyGem(Vector2 gridPos) {
            var gem = GetGemByGridPos(gridPos);
            if (gem != null){
                Gems.Remove(gem);

                gem.ActionBonus(this);

                Destroy(gem.gameObject);
                _player.AddScore(Constants.ScorePerGem);
            }
        }

        public void Swap(GemLogic gem1, GemLogic gem2) {
            var tmpPos = gem1.GridPos;
            MoveGem(gem1, gem2.GridPos);
            MoveGem(gem2, tmpPos);

            _lastMovedGem1 = gem1;
            _lastMovedGem2 = gem2;
            Invoke("CheckReverseTurn", Constants.GemTransitionTime/2);
            
            SelectGem(null);
            _updatingGridCounter = Constants.GemTransitionTime;
        }

        public void CheckReverseTurn() {
            if (!GetMatchesGem().Any()) {
                var tmpPos = _lastMovedGem1.GridPos;
                MoveGem(_lastMovedGem1, _lastMovedGem2.GridPos);
                MoveGem(_lastMovedGem2, tmpPos);
            }
        }

        private void SelectGem(GemLogic gem) {
            _choosen = gem;
            if (gem == null){
                _choosenObject.transform.position = new Vector2(-10, -10);
            }
            else{
                var screenPos = Utils.GetScreenPosByGrid(gem.GridPos);
                var newPos = _camera.ScreenToWorldPoint(screenPos);
                newPos.z = 0;
                _choosenObject.transform.position = newPos;
            }
        }


        public List<List<GemLogic>> GetMatchesGem() {
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
            // check rows:
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

            return matches;
        }

        public GemLogic GetGemByGridPos(Vector2 gridPos) {
            try{
                var gem = Gems.First(g=>g.GridPos == gridPos);
                return gem;
            }
            catch (InvalidOperationException){
                return null;
            }
        }
    }
}