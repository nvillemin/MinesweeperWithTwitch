using UnityEngine;
using Global;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
	public float time { get; private set; }

    private Text text;
    private bool isActive;

    // ========================================================================
    // Timer initialization
    void Start() {
        this.text = (Text)this.GetComponent("Text");
        this.time = 0;
        this.isActive = false;
    }

    // ========================================================================
    // Updating the timer once per frame if it's active
    void Update() {
        if (this.isActive) {
            this.time += Time.deltaTime;
            this.text.text = GlobalManager.TimeToString(this.time);
        }
    }

    // ========================================================================
    // Start the timer, called when mine have been generated
    public void StartTimer() {
        this.isActive = true;
    }

    // ========================================================================
    // Stop the timer at the end of the game
    public void StopTimer() {
        this.isActive = false;
    }
}
