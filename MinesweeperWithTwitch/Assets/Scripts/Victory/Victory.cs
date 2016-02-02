using UnityEngine;
using System.Collections;

public class Victory : MonoBehaviour {
	public TextMesh tmScore, tmDeaths;

	// Victory scene initialization
	void Start () {
		this.tmScore.text = GlobalManager.endScore.ToString("f3"); // f3 means 3 decimals
        this.tmDeaths.text = GlobalManager.endDeaths.ToString();
	}
}
