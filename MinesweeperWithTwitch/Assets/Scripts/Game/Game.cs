using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {
    public GameObject squarePrefab, borderSquarePrefab, borderTextPrefab;
    public TextMesh tmDeaths, tmDeadList;
    public Timer timer;
    public bool isGameMined { get; private set; }
    public int nbSquaresX, nbSquaresY, nbMines;
    public float deadTime;

    private Square[,] squares;
    private Dictionary<string, float> deadUsersTime;
    private int nbSquaresChecked, nbDeaths;

    // Game initialization
    void Start() {
        this.isGameMined = false;
        this.nbSquaresChecked = 0;
        this.nbDeaths = 0;
        this.squares = new Square[this.nbSquaresX, this.nbSquaresY];
        this.deadUsersTime = new Dictionary<string, float>();
        this.CreateBorders();
        this.CreateField();
    }

    // Create the borders of the game field
    private void CreateBorders() {
        // Create the top and bottom border squares
        for (int i = 0; i < this.nbSquaresX + 2; i++) {
            foreach (float pos in new float[] { 0f, 0.36f * (this.nbSquaresY + 1) }) {
                GameObject borderSquare = (GameObject)Instantiate(this.borderSquarePrefab, new Vector3(0.28f + (float)i * 0.36f, -0.28f - pos, 5), Quaternion.identity);

                // Create the number in the square
                if (i > 0 && i < this.nbSquaresX + 1) {
                    GameObject nbMinesTextTop = (GameObject)Instantiate(this.borderTextPrefab, borderSquare.transform.position, this.transform.rotation);
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

    // Generate the mines of the field
    // Mines are only generated after one square has been checked to prevent it from being a mine
    public void GenerateMines(Square checkedSquare) {
        int nbMinesGenerated = 0;

        // Generating mines until the number of mines we want has been reached
        while (nbMinesGenerated < this.nbMines) {
            // Generating a random position for the mine
            int indexX = Random.Range(0, this.nbSquaresX);
            int indexY = Random.Range(0, this.nbSquaresY);

            // Checking that this square isn't the one we clicked on and that it doesn't contain a mine already
            if (!this.squares[indexX, indexY].isMined && this.squares[indexX, indexY] != checkedSquare) {
                // Adding the mine on the square
                this.squares[indexX, indexY].AddMine();
                nbMinesGenerated++;
            }
        }

        // The game is now mined, we can start the timer
        this.isGameMined = true;
        this.timer.StartTimer();
    }
    
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
        if(this.deadUsersTime.Count > 0) {
            string deadPlayersList = string.Empty;
            deadUsers = new List<string>(this.deadUsersTime.Keys);
            foreach (string user in deadUsers) {
                deadPlayersList += user + " - " + this.deadUsersTime[user].ToString("f1") + "\n";
            }
            this.tmDeadList.text = deadPlayersList;
        } else {
            this.tmDeadList.text = "nobody!";
        }
    }

    // Called by a user command in Twitch chat
    public void ChatCommand(string user, string command, int x, int y) {
        if(x >= 0 && x < this.nbSquaresX && y >= 0 && y < this.nbSquaresY && !this.deadUsersTime.ContainsKey(user)) {
            if (command.Equals("check")) {
                if(this.squares[x, y].Check()) {
                    // User checked a mine, kill him for some time
                    this.deadUsersTime.Add(user, this.deadTime);
                    this.nbDeaths++;
                    this.tmDeaths.text = this.nbDeaths.ToString();
                }
            } else if(command.Equals("flag")) {
                this.squares[x, y].Flag();
            } else if(command.Equals("unflag")) {
                this.squares[x, y].Unflag();
            }
        }
    }

    // Called when a new square has been checked
    public void NewSquareChecked() {
        this.nbSquaresChecked++;
        if (this.nbSquaresChecked >= (this.nbSquaresX * this.nbSquaresY - this.nbMines)) {
            this.Victory();
        }
    }

    // Victory, called when all mines have been found
    public void Victory() {
        this.timer.StopTimer();
		GlobalManager.endScore = this.timer.time;
        GlobalManager.endDeaths = this.nbDeaths;
		SceneManager.LoadScene("Victory");
    }
}
