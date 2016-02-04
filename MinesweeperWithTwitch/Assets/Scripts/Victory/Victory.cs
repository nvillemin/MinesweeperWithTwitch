﻿using UnityEngine;
using Global;
using System.Collections.Generic;
using System.IO;

public class Victory : MonoBehaviour {
	public TextMesh tmScore, tmDeaths, tmScoresList, tmTimesList;

    // ========================================================================
    // Victory scene initialization
    private void Start () {
		this.tmScore.text = GlobalManager.endScore.ToString("f3"); // f3 means 3 decimals
        this.tmDeaths.text = GlobalManager.endDeaths.ToString();

        List<KeyValuePair<string, int>> orderedScores = GlobalManager.orderedScores;
        string scoresList = string.Empty;
        int count = 0;
        while (count < orderedScores.Count && count < 9) {
            scoresList += orderedScores[count].Key + " - " + orderedScores[count].Value.ToString() + "\n";
            count++;
        }
        this.tmScoresList.text = scoresList;
		this.tmTimesList.text = this.GetBestTimes();
	}

	// ========================================================================
	// Get the best times from the file
	private string GetBestTimes() {
		string[] lines = File.ReadAllLines("Data/bestTimes.txt");
		string timesText = string.Empty;

		foreach(string line in lines) {
			string newLine = line.Replace(";", ") ");
			timesText += "(" + newLine + "\n";
		}

		return timesText;
	}
}
