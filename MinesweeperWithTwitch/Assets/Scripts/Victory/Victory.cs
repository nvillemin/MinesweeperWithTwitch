using UnityEngine;
using Global;
using System.Collections.Generic;

public class Victory : MonoBehaviour {
	public TextMesh tmScore, tmDeaths, tmScoresList;

    // ========================================================================
    // Victory scene initialization
    void Start () {
		this.tmScore.text = GlobalManager.endScore.ToString("f3"); // f3 means 3 decimals
        this.tmDeaths.text = GlobalManager.endDeaths.ToString();

        List<KeyValuePair<string, int>> orderedScores = GlobalManager.orderedScores;
        string scoresList = string.Empty;
        int count = 0;
        while (count < orderedScores.Count && count < 9) {
            scoresList += orderedScores[count].Key + " - " + orderedScores[count].Value.ToString();
            count++;
        }
        this.tmScoresList.text = scoresList;
	}
}
