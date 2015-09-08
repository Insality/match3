using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour {


    public int Score;
    public Text ScoreText;

    private int _curGuiScore;
	void Start () {
	    Score = 0;
	    _curGuiScore = 0;
	}


    public void AddScore(int score) {
        Score += score;
    }

    public void Restart() {
        Score = 0;
        _curGuiScore = 0;
    }

	void Update () {

	    if (_curGuiScore < Score){
	        _curGuiScore += 9;
	    }
	    else{
	        _curGuiScore = Score;
	    }

	    ScoreText.text = _curGuiScore.ToString();
	}
}
