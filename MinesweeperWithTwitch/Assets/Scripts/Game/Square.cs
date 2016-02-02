using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour {
    public Sprite checkedSprite;
    public GameObject nbMinesAdjText, flagPrefab, minePrefab;

    public int nbMinesAdj { get; set; }
    public bool isMined { get; private set; }
    public bool isChecked { get; private set; }
    public bool isFlagged { get; private set; }

    private Game game;
    private SpriteRenderer spriteRenderer;
    private GameObject flag, mine;
    private List<Square> neighbors;

    private bool isRightButtonDown;
    public int indexX, indexY;

    // Need to create the list right when the square is being instanciated since we will add neighbors
    void Awake() {
        this.neighbors = new List<Square>();
    }

    // Square initialization
    void Start() {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.isMined = false;
        this.isFlagged = false;
        this.isChecked = false;
        this.isRightButtonDown = false;
        this.nbMinesAdj = 0;
    }

    // Initialization of the reference of the game and the indexes
    public void Initialize(Game game, int x, int y) {
        this.game = game;
        this.indexX = x;
        this.indexY = y;
    }

    // Neighbors initialization
    public void AddNeighbor(Square neighbor) {
        this.neighbors.Add(neighbor);
        neighbor.neighbors.Add(this);
    }

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

    // Left click on the square
    // TODO : Remove this after
    void OnMouseDown() {
        this.game.ChatCommand("ADMIN", "check", this.indexX, this.indexY);
    }

    // Add a mine on this square
    public void AddMine() {
        this.isMined = true;
        foreach(Square neighbor in this.neighbors) {
            neighbor.nbMinesAdj++;
        }
    }

    // Check this square
    // Returns true if the user checked a mine
    public bool Check() {
        if(!this.isChecked && !this.isFlagged) {
            // Generate mines if the field doesn't contain any
            if(!this.game.isGameMined) {
                this.game.GenerateMines(this);
            }

            this.isChecked = true;
            this.spriteRenderer.sprite = this.checkedSprite;

            // There is a mine on this square, defeat
            if (this.isMined) {
                this.mine = (GameObject)Instantiate(this.minePrefab, this.transform.position, Quaternion.identity);
                return true;
            // There are mines nearby, display the number
            } else if (this.nbMinesAdj > 0) {
                GameObject nbMinesText = (GameObject)Instantiate(this.nbMinesAdjText, this.transform.position, this.transform.rotation);
                TextMesh tm = (TextMesh)nbMinesText.GetComponent("TextMesh");
                tm.text = this.nbMinesAdj.ToString();
                tm.color = GlobalManager.minesTextColor[nbMinesAdj - 1]; // -1 because array starts at 0
            // No mine and also no mines nearby, check the neighbors as well
            } else {
                foreach (Square neighbor in this.neighbors) {
                    if (!neighbor.isChecked) {
                        neighbor.Check();
                    }
                }
            }
            this.game.NewSquareChecked();
        }
        return false;
    }

    // Flag this square
    public void Flag() {
        if(!this.isChecked && !this.isFlagged) {
            this.flag = (GameObject)Instantiate(this.flagPrefab, this.transform.position, Quaternion.identity);
            this.isFlagged = true;
        }
    }

    // Unflag this square
    public void Unflag() {
        if (!this.isChecked && this.isFlagged) {
            Destroy(this.flag);
            this.isFlagged = false;
        }
    }
}
