using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartTimer : MonoBehaviour {
    public float restartTime;

	private TextMesh tm;
	private float time;

    // ========================================================================
    // Restart timer initialization
    void Start() {
		this.tm = (TextMesh)this.GetComponent("TextMesh");
        this.tm.text = restartTime.ToString();
        this.time = restartTime;
	}

    // ========================================================================
    // Updating the timer once per frame
    void Update() {
		this.time -= Time.deltaTime;
		if(this.time <= 0f) {
			this.tm.text = "GO!";
			SceneManager.LoadScene("Game");
		} else {
			this.tm.text = (Math.Ceiling(time)).ToString(); // f3 means 3 decimals
		}
	}
}
