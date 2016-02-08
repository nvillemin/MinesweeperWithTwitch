using UnityEngine;
using Global;
using System.Collections.Generic;

public class ScoreEvent : MonoBehaviour {
    public bool isPlanned;
    public float activeTime;
    public bool gameStarted { get; set; }

    private Game game;
    private Square chosenSquare;
    private float timer, activeTimer;
    private bool isActive;

    // ========================================================================
    // Score event initialization
    private void Start() {
        this.gameStarted = false;
        this.activeTimer = 0f;
        this.isActive = false;
    }

    // ========================================================================
    // Starting the time when the first check has been done
    public void StartGame(Game game) {
        if (this.isPlanned) {
            this.game = game;
            this.timer = UnityEngine.Random.Range(120f, 3600f);
            this.gameStarted = true;
        }
    }

    // ========================================================================
    // Updating the timer once per frame
    private void Update () {
	    if(this.isPlanned && this.gameStarted) {
            this.timer -= Time.deltaTime;
            if(this.timer <= 0f) {
                this.isPlanned = false;
                this.Activate();
            }
        } else if(this.isActive) {
            this.activeTimer += Time.deltaTime;
            if (this.activeTimer >= activeTime) {
                this.isActive = false;
                this.chosenSquare.SetScoreEvent(true, this.isActive);
            }
        }
	}

    // ========================================================================
    // Timer is over, launching the event
    private void Activate() {
        List<Square> availableSquares = new List<Square>();
        foreach(Square square in this.game.squares) {
            if(!square.isChecked && !square.isFlagged) {
                List<Square> neighbors = square.GetNeighbors();
                bool uncheckedNeighbors = true;
                int i = 0;
                while (i < neighbors.Count && uncheckedNeighbors) {
                    if(neighbors[i].isChecked || neighbors[i].isFlagged) {
                        uncheckedNeighbors = false;
                    }
                    i++;
                }
                
                if(uncheckedNeighbors) {
                    availableSquares.Add(square);
                }
            }
        }

        int nbAvailableSquares = availableSquares.Count;
        if (nbAvailableSquares != 0) {
            this.isActive = true;
            this.chosenSquare = availableSquares[UnityEngine.Random.Range(0, nbAvailableSquares)];
            this.chosenSquare.SetScoreEvent(true, this.isActive);
            GlobalManager.twitch.SendMsg("Kreygasm Score event incoming! Check the green squares to get 5 points instead of 1! This event will only last two minutes.");
        }
    }
}
