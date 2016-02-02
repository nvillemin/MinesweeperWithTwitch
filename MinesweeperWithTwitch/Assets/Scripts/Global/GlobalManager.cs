using UnityEngine;

// Static class used to store global variables
public static class GlobalManager {
    // Text colors for the number of mines nearby
    public static readonly Color[] minesTextColor = {
        new Color(0, 0.35f, 1), // 1
        new Color(0, 0.51f, 0.09f), // 2
        new Color(0.78f, 0, 0), // 3
        new Color(0, 0, 0.61f), // 4
        new Color(0.47f, 0, 0), // 5
        new Color(0.29f, 0.55f, 0.55f), // 6
        new Color(0.23f, 0.23f, 0.23f), // 7
        Color.black // 8
    };

	// Values for the end of the game
	public static float endScore;
    public static int endDeaths;
}
