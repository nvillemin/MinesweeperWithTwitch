using System;
using System.Collections.Generic;

[Serializable]
public class UserScores {
	public Dictionary<string, int> scores { get; private set; }

	// ========================================================================
	public UserScores() {
		this.scores = new Dictionary<string, int>();
	}

	// ========================================================================
	public void AddScore(string user, int score) {
		if(this.scores.ContainsKey(user)) {
			this.scores[user] += score;
		} else {
			this.scores.Add(user, score);
		}
	}
}