using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour {


    public int Score;
    public Text ScoreText;

    private int _curGUIScore;
	// Use this for initialization
	void Start () {
	    Score = 0;
	    _curGUIScore = 0;
	}


    public void AddScore(int score) {
        Score += score;
    }

	// Update is called once per frame
	void Update () {

	    if (_curGUIScore < Score){
	        _curGUIScore += 9;
	    }
	    else{
	        _curGUIScore = Score;
	    }

	    ScoreText.text = _curGUIScore.ToString();
	}
}
