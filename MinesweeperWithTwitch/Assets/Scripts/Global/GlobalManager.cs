using System.Collections.Generic;
using UnityEngine;

namespace Global {
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
        public static string endTime;
        public static int endDeaths;
        public static List<KeyValuePair<string, int>> gameScores;
		public static UserScores globalScores;

        // ========================================================================
        // Convert a float to a string with hours, minutes and seconds
        public static string TimeToString(float time) {
            int hours = (int)time / 3600;
            int minutes = (int)(time / 60) % 60;
            int seconds = (int)time % 60;

            string timeString = string.Empty;
            if (hours > 0) {
                timeString += hours.ToString() + "h ";
            }
            if (minutes > 0) {
                timeString += minutes.ToString() + "m ";
            }
            timeString += seconds.ToString() + "s";

            return timeString;
        }
    }
}