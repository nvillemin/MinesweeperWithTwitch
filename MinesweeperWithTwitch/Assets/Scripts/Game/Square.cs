﻿using Global;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour {
    public GameObject nbNearbyMinesText, coordinatesText, flagPrefab, minePrefab;

    public int nbNearbyMines { get; set; }
    public bool isMined { get; private set; }
    public bool isChecked { get; private set; }
    public bool isFlagged { get; private set; }

    private Game game;
    private SpriteRenderer spriteRenderer;
    private GameObject flag, coordinates;
    private List<Square> neighbors = new List<Square>();
	private bool isRightButtonDown, isScoreEvent;
    private int indexX, indexY;

    // ========================================================================
    // Square initialization
    void Start() {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.isMined = false;
        this.isFlagged = false;
        this.isChecked = false;
        this.isRightButtonDown = false;
        this.isScoreEvent = false;
        this.nbNearbyMines = 0;
    }

    // ========================================================================
    // Initialization of the reference of the game and the indexes
    public void Initialize(Game game, int x, int y) {
        this.game = game;
        this.indexX = x;
        this.indexY = y;
		this.CreateCoordinatesText();
	}

	// ========================================================================
	// Creates the white text with the coordinates of the square
	private void CreateCoordinatesText() {
		this.coordinates = (GameObject)Instantiate(this.coordinatesText, this.transform.position - new Vector3(0, 0, 1), this.transform.rotation);
		TextMesh tm = (TextMesh)coordinates.GetComponent("TextMesh");
		char letter = (char)(this.indexY + 65);
		tm.text = letter.ToString() + (this.indexX + 1).ToString();
	}

    // ========================================================================
    // Neighbors initialization
    public void AddNeighbor(Square neighbor) {
        this.neighbors.Add(neighbor);
        neighbor.neighbors.Add(this);
    }

    // ========================================================================
    // Right click detection
    // TODO : Remove this after
    void OnMouseOver() {
        if (!this.isChecked) {
            if (Input.GetMouseButton(1) && !this.isRightButtonDown) {
                this.game.ChatCommand("ADMIN", "flag", this.indexX, this.indexY);
                this.isRightButtonDown = true;
            } else if (!Input.GetMouseButton(1) && this.isRightButtonDown) {
                this.isRightButtonDown = false;
            }
        }
    }

    // ========================================================================
    // Left click on the square
    // TODO : Remove this after
    void OnMouseDown() {
        this.game.ChatCommand("ADMIN", "check", this.indexX, this.indexY);
    }

    // ========================================================================
    // Add a mine on this square
    public void AddMine() {
        this.isMined = true;
        foreach(Square neighbor in this.neighbors) {
            neighbor.nbNearbyMines++;
        }
    }

	// ========================================================================
	// Check this square
	public KeyValuePair<int, int> Check(KeyValuePair<int, int> checkValues, string user) {
		if(!this.isChecked && !this.isFlagged) {
			// Generate the mines if it's the first check of the game
			if(!this.game.isGameMined) {
				if(this.game.nbMines >= this.game.nbSquares - 9) {
					throw new System.Exception("Too many mines!");
				} else {
					this.game.GenerateMines(this);
				}
			}

			Destroy(this.coordinates);
			this.isChecked = true;
            this.spriteRenderer.color = new Color(1, 1, 1);
            //int pointsAwarded = (this.isScoreEvent) ? 5 : 1;

			// There is a mine on this square
			if(this.isMined) {
				Instantiate(this.minePrefab, this.transform.position - new Vector3(0, 0, 1), Quaternion.identity);
                this.spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f);
				this.game.KillUser(user, this.indexX, this.indexY);
				return new KeyValuePair<int, int>(checkValues.Key, checkValues.Value + 1);
			// There are mines nearby, display the number
			} else if(this.nbNearbyMines > 0) {
				GameObject nbMinesText = (GameObject)Instantiate(this.nbNearbyMinesText, this.transform.position - new Vector3(0, 0, 1), this.transform.rotation);
				TextMesh tm = (TextMesh)nbMinesText.GetComponent("TextMesh");
				tm.text = this.nbNearbyMines.ToString();
				tm.color = GlobalManager.minesTextColor[nbNearbyMines - 1]; // -1 because array starts at 0
				return new KeyValuePair<int, int>(checkValues.Key + 1, checkValues.Value);
			// No mine and also no mines nearby, check the neighbors as well
			} else {
				checkValues = new KeyValuePair<int, int>(checkValues.Key + 1, checkValues.Value);
				foreach(Square neighbor in this.neighbors) {
					if(!neighbor.isChecked) {
						checkValues = neighbor.Check(checkValues, user);
					}
				}
			}
		}
		return checkValues;
	}

	// ========================================================================
	// Flag this square
	public void Flag() {
        if(!this.isChecked && !this.isFlagged) {
			Destroy(this.coordinates);
            this.flag = (GameObject)Instantiate(this.flagPrefab, this.transform.position - new Vector3(0, 0, 1), Quaternion.identity);
			this.isFlagged = true;
        }
    }

    // ========================================================================
    // Unflag this square
    public void Unflag() {
        if (!this.isChecked && this.isFlagged) {
            Destroy(this.flag);
			this.CreateCoordinatesText();
			this.isFlagged = false;
        }
    }

	// ========================================================================
	// Clear this square if all nearby mines have been flagged
	public void Clear(string user) {
		if (this.isChecked) {
			// Check the number of flags or checked bombs in the nearby squares
			int nearbyFlags = 0;
			foreach(Square neighbor in this.neighbors) {
				if(neighbor.isFlagged || (neighbor.isChecked && neighbor.isMined)) {
					nearbyFlags++;
				}
			}

			// Clear is possible, check neighbors
			if(nearbyFlags == this.nbNearbyMines) {
				// Check neighbors and get the number of successful checks and the number of bombs checked
				KeyValuePair<int, int> checkValues = new KeyValuePair<int, int>(0, 0);
				foreach(Square neighbor in this.neighbors) {
					if(!neighbor.isChecked) {
						checkValues = neighbor.Check(checkValues, user);
					}
				}

				// No bombs checked, increase user score
				if(checkValues.Value == 0) {
					this.game.IncrementUserScore(user, checkValues.Key);
				}
				this.game.NewSquaresChecked(checkValues.Key);
			}
		}
	}

    // ========================================================================
    // Tag or untag this square has the chosen one for the score event
    public void SetScoreEvent(bool main, bool active) {
        this.isScoreEvent = active;
        if(!this.isChecked) {
            this.spriteRenderer.color = (active) ? new Color(0, 0.8f, 0) : new Color(0.3529f, 0.6549f, 1);
        }
        if(main) {
            foreach(Square neighbor in this.neighbors) {
                neighbor.SetScoreEvent(false, active);
            }
        }
    }

    public List<Square> GetNeighbors() { return this.neighbors;  }
}
