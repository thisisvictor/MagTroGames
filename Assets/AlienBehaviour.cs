using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienBehaviour : MonoBehaviour {

	public int row, col;
	public bool isAlive = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//Handle the case when it's been hit by a bullet
	public void HandleHit() {
		isAlive = false;

		//disable Sprite Renderer
		GetComponent<Renderer>().enabled = false;
		//disable the 2D Collider
		GetComponent<Collider2D>().enabled = false;
	}

	//Hanlde the case when it's been resurrected
	public void HandleResurrect() {
		isAlive = true;

		//disable Sprite Renderer
		GetComponent<Renderer>().enabled = true;
		//disable the 2D Collider
		GetComponent<Collider2D>().enabled = true;
	}
}
