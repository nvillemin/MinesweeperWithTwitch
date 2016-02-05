using UnityEngine;
using Global;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Victory : MonoBehaviour {
	public TextMesh tmScore, tmDeaths, tmScoresList, tmTimesList, tmBestPlayersList;

    // ========================================================================
    // Victory scene initialization
    private void Start () {
		this.tmScore.text = GlobalManager.endTime; // f3 means 3 decimals
        this.tmDeaths.text = GlobalManager.endDeaths.ToString();

        List<KeyValuePair<string, int>> orderedScores = GlobalManager.gameScores;
        string scoresList = string.Empty;
        int count = 0;
        while (count < orderedScores.Count && count < 9) {
            scoresList += orderedScores[count].Key + " - " + orderedScores[count].Value.ToString() + "\n";
            count++;
        }
        this.tmScoresList.text = scoresList;
		this.tmTimesList.text = this.GetBestTimes();
		this.tmBestPlayersList.text = this.GetBestPlayers();
	}

	// ========================================================================
	// Get the best times from the file
	private string GetBestTimes() {
		string[] lines = File.ReadAllLines("Data/bestTimes.txt");
		string timesText = string.Empty;

		foreach(string line in lines) {
            string[] parts = line.Split(';');
            timesText += "(" + parts[0] + ") " + GlobalManager.TimeToString(float.Parse(parts[1])) + "\n";
		}

		return timesText;
	}

	// ========================================================================
	// Get the best all time players
	private string GetBestPlayers() {
		List<KeyValuePair<string, int>> bestPlayers = GlobalManager.globalScores.scores.OrderByDescending(x => x.Value).ToList();
		string playersList = string.Empty;
		int count = 0;
		while(count < bestPlayers.Count && count < 10) {
			playersList += bestPlayers[count].Key + " - " + bestPlayers[count].Value.ToString() + "\n";
			count++;
		}

		return playersList;
	}
}
