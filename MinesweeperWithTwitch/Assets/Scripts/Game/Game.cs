using Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {
	public GameObject squarePrefab, borderSquarePrefab, borderTextPrefab;
    public TextMesh tmDeaths, tmScoresList, tmDeadList, tmChecked;
    public Timer timer;
    public bool isGameMined { get; private set; }
    public int nbSquares, nbMines, nbBestTimes;
    public float deadTime;

    private Square[,] squares;
    private Dictionary<string, float> deadUsersTime;
    private Dictionary<string, int> userScores;
    private int nbSquaresX, nbSquaresY, nbSquaresChecked, nbSquaresNeeded, nbDeaths;

    // ========================================================================
    // Game initialization
    void Start() {
        this.nbSquaresX = this.nbSquares / 14;
        this.nbSquaresY = this.nbSquares / 20;
        this.isGameMined = false;
		this.nbSquaresNeeded = (this.nbSquaresX * this.nbSquaresY) - this.nbMines;
        this.nbSquaresChecked = 0;
		this.tmChecked.text = this.nbSquaresChecked.ToString() + " / " + this.nbSquaresNeeded.ToString();
		this.nbDeaths = 0;
        this.squares = new Square[this.nbSquaresX, this.nbSquaresY];
        this.deadUsersTime = new Dictionary<string, float>();
        this.userScores = new Dictionary<string, int>();
        this.CreateBorders();
        this.CreateField();
		GlobalManager.globalScores = this.InitializeGlobalScores();
    }

    // ========================================================================
    // Create the borders of the game field
    private void CreateBorders() {
        // Create the top and bottom border squares
        for (int i = 0; i < this.nbSquaresX + 2; i++) {
            foreach (float pos in new float[] { 0f, 0.36f * (this.nbSquaresY + 1) }) {
                GameObject borderSquare = (GameObject)Instantiate(this.borderSquarePrefab, new Vector3(0.28f + (float)i * 0.36f, -0.28f - pos, 5), Quaternion.identity);

                // Create the number in the square
                if (i > 0 && i < this.nbSquaresX + 1) {
                    GameObject nbMinesTextTop = (GameObject)Instantiate(this.borderTextPrefab, borderSquare.transform.position - new Vector3(0, 0, 1), this.transform.rotation);
                    ((TextMesh)nbMinesTextTop.GetComponent("TextMesh")).text = i.ToString();
                }
            }
        }

        // Create the left and right border squares
        for (int j = 0; j < this.nbSquaresY + 2; j++) {
            foreach (float pos in new float[] { 0f, 0.36f * (this.nbSquaresX + 1) }) {
                GameObject borderSquare = (GameObject)Instantiate(this.borderSquarePrefab, new Vector3(0.28f + pos, -0.28f - (float)j * 0.36f, 5), Quaternion.identity);

                // Create the number in the square
                if (j > 0 && j < this.nbSquaresY + 1) {
                    GameObject nbMinesTextTop = (GameObject)Instantiate(this.borderTextPrefab, borderSquare.transform.position, this.transform.rotation);
                    ((TextMesh)nbMinesTextTop.GetComponent("TextMesh")).text = ((char)(64 + j)).ToString();
                }
            }
        }
    }

    // ========================================================================
    // Create the game field
    private void CreateField() {
        // Creating all the squares for the mines
        for (int j = 0; j < this.nbSquaresY; j++) {
            for (int i = 0; i < this.nbSquaresX; i++) {
                // Creating the square
                this.squares[i, j] = (Square)((GameObject)Instantiate(this.squarePrefab, new Vector3(0.64f + (float)i * 0.36f, -0.64f + (float)j * -0.36f, 5), Quaternion.identity)).GetComponent("Square");
                this.squares[i, j].Initialize(this, i, j);

                // Adding the neighbors
                if (i > 0) {
                    this.squares[i, j].AddNeighbor(this.squares[i - 1, j]);
                    if (j > 0) {
                        this.squares[i, j].AddNeighbor(this.squares[i - 1, j - 1]);
                    }
                }
                if (j > 0) {
                    this.squares[i, j].AddNeighbor(this.squares[i, j - 1]);
                    if (i < this.nbSquaresX - 1) {
                        this.squares[i, j].AddNeighbor(this.squares[i + 1, j - 1]);
                    }
                }
            }
        }
    }

	// ========================================================================
	// Deserialize the all time player scores
	private UserScores InitializeGlobalScores() {
		if(File.Exists("Data/UserScores")) {
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream("Data/UserScores", FileMode.Open, FileAccess.Read, FileShare.Read);
			UserScores scores = (UserScores)formatter.Deserialize(stream);
			stream.Close();
			return scores;
		}
		return new UserScores();
	}

	// ========================================================================
	// Generate the mines of the field
	// Mines are only generated after one square has been checked to prevent it from being a mine
	public void GenerateMines(Square checkedSquare) {
        int nbMinesGenerated = 0;

        // Generating mines until the number of mines we want has been reached
        while (nbMinesGenerated < this.nbMines) {
            // Generating a random position for the mine
            int indexX = UnityEngine.Random.Range(0, this.nbSquaresX);
            int indexY = UnityEngine.Random.Range(0, this.nbSquaresY);

            // Checking that this square isn't the one we clicked on and that it doesn't contain a mine already
            if (!this.squares[indexX, indexY].isMined && this.squares[indexX, indexY] != checkedSquare
				&& !checkedSquare.GetNeighbors().Contains(this.squares[indexX, indexY])) {
                // Adding the mine on the square
                this.squares[indexX, indexY].AddMine();
                nbMinesGenerated++;
            }
        }

        // The game is now mined, we can start the timer
        this.isGameMined = true;
        this.timer.StartTimer();
    }
    
    // ========================================================================
    // Called each frame, used to update the dead players timers
    private void Update() {
        // Update the timer of all players and remove the ones with a timer < 0
        List<string> deadUsers = new List<string>(this.deadUsersTime.Keys);
        foreach (string user in deadUsers) {
            this.deadUsersTime[user] -= Time.deltaTime;
            if (this.deadUsersTime[user] < 0f) {
                this.deadUsersTime.Remove(user);
            }
        }

        // Update the dead players list
        if (this.deadUsersTime.Count > 0) {
            string deadPlayersList = string.Empty;
            deadUsers = new List<string>(this.deadUsersTime.Keys);
            int count = 0;
            while (count < deadUsers.Count && count < 5) {
                deadPlayersList += deadUsers[count] + " - " + this.deadUsersTime[deadUsers[count]].ToString("f1") + "\n";
                count++;
            }
            this.tmDeadList.text = deadPlayersList;
        } else {
            this.tmDeadList.text = "-";
        }
    }

    // ========================================================================
    // Update the score list, called when a player score changed
    public void UpdateScoreList() {
        if(this.userScores.Count > 0) {
            List<KeyValuePair<string, int>> orderedScores = this.userScores.OrderByDescending(x => x.Value).ToList();
            string scoresList = string.Empty;
            int count = 0;
            while (count < orderedScores.Count && count < 5) {
                scoresList += orderedScores[count].Key + " - " + orderedScores[count].Value.ToString() + "\n";
                count++;
            }
            this.tmScoresList.text = scoresList;
        } else {
            this.tmDeadList.text = "-";
        }        
    }

    // ========================================================================
    // Called by a user command in Twitch chat
    public void ChatCommand(string user, string command, int x, int y) {
        if(x >= 0 && x < this.nbSquaresX && y >= 0 && y < this.nbSquaresY && !this.deadUsersTime.ContainsKey(user)) {
            if (command.Equals("check")) {
				KeyValuePair<int, int> checkValues = this.squares[x, y].Check(new KeyValuePair<int, int>(0, 0));
				// User checked a mine, kill him for some time
				if(checkValues.Value > 0) {
					this.KillUser(user);
					this.UpdateScoreList();
				} else {
					this.IncrementUserScore(user, checkValues.Key);
					this.NewSquaresChecked(checkValues.Key + checkValues.Value);
				}
            } else if(command.Equals("flag")) {
                this.squares[x, y].Flag();
            } else if(command.Equals("unflag")) {
                this.squares[x, y].Unflag();
            } else if(command.Equals("clear")) {
				this.squares[x, y].Clear(user);
			}
		}
    }

    // ========================================================================
    // Called when new squares have been checked
    public void NewSquaresChecked(int nbSquares) {
		// Check if all the squares have been checked
		this.nbSquaresChecked += nbSquares;
		this.tmChecked.text = this.nbSquaresChecked.ToString() + " / " + this.nbSquaresNeeded.ToString();
		if (this.nbSquaresChecked >= this.nbSquaresNeeded) {
            this.Victory();
        }
    }

	// ========================================================================
	// Called when a user succesfully checked squares
	public void IncrementUserScore(string user, int score) {
		if(this.userScores.ContainsKey(user)) {
			this.userScores[user] += score;
		} else {
			this.userScores.Add(user, score);
		}
		this.UpdateScoreList();
	}

	// ========================================================================
	// Called when a user checks a bomb, decreases his score by 25%
	public void KillUser(string user) {
		if(!this.deadUsersTime.Keys.Contains(user)) {
			this.deadUsersTime.Add(user, this.deadTime);
		} else {
			this.deadUsersTime[user] = this.deadTime;
		}
		
		this.nbDeaths++;
		this.tmDeaths.text = this.nbDeaths.ToString();
		if(this.userScores.ContainsKey(user)) {
			this.userScores[user] = (int)Math.Floor(this.userScores[user] * 0.75f);
		} else {
			this.userScores.Add(user, 0);
		}
	}

    // ========================================================================
    // Victory, called when all mines have been found
    public void Victory() {
        this.timer.StopTimer();
		this.RegisterTime();
		this.RegisterScores();
		GlobalManager.endScore = this.timer.time;
        GlobalManager.endDeaths = this.nbDeaths;
        GlobalManager.gameScores = this.userScores.OrderByDescending(x => x.Value).ToList();
        SceneManager.LoadScene("Victory");
    }

	// ========================================================================
	// Registering the time spent to complete the game and the number of deaths
	private void RegisterTime() {
		string newTime = this.nbDeaths.ToString() + ";" + this.timer.time.ToString("f3") + "\n";
		FileInfo timesFile = new FileInfo("Data/bestTimes.txt");
		timesFile.Directory.Create();

		if(timesFile.Exists) {
			string[] lines = File.ReadAllLines(timesFile.FullName);
			int nbLines = lines.Length;
			List<KeyValuePair<int, float>> lineValues = new List<KeyValuePair<int, float>>();

			for(int i=0; i< nbLines; ++i) {
				string[] values = lines[i].Split(';');
				lineValues.Add(new KeyValuePair<int, float>(Int32.Parse(values[0]), float.Parse(values[1])));
			}

			bool newBestTime = false;
			if(nbLines < this.nbBestTimes) {
				lineValues.Add(new KeyValuePair<int, float>(this.nbDeaths, this.timer.time));
				lineValues = lineValues.OrderBy(x => x.Key).ThenBy(x => x.Value).ToList();
				newBestTime = true;
			} else {
				int worstDeaths = lineValues[nbLines - 1].Key;
				float worstTime = lineValues[nbLines - 1].Value;

				if(worstDeaths > this.nbDeaths || (worstDeaths == this.nbDeaths && worstTime > this.timer.time)) {
					lineValues[this.nbBestTimes - 1] = new KeyValuePair<int, float>(this.nbDeaths, this.timer.time);
					lineValues = lineValues.OrderBy(x => x.Key).ThenBy(x => x.Value).ToList();
					newBestTime = true;
				}
			}

			if(newBestTime) {
				File.WriteAllText(timesFile.FullName, string.Empty);
				foreach(KeyValuePair<int, float> values in lineValues) {
					File.AppendAllText(timesFile.FullName, values.Key.ToString() + ";" + values.Value.ToString("f3") + "\n");
				}
			}
		} else {
			File.WriteAllText(timesFile.FullName, newTime);
		}
	}

	// ========================================================================
	// Registering the scores of all players, called at the end of the game
	private void RegisterScores() {
		// Update the score of all players of this game
		foreach(string user in this.userScores.Keys) {
			GlobalManager.globalScores.AddScore(user, this.userScores[user]);
		}

		// Serialize the scores to keep them for later
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream("Data/UserScores", FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, GlobalManager.globalScores);
		stream.Close();
	}
}
