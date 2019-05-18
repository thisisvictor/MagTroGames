using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterBulletBehaviour : MonoBehaviour {

	//gameplay parameters
	float upSpeed = 1f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		GetComponent<Rigidbody2D> ().velocity = new Vector2 (-Mathf.Sin (transform.rotation.eulerAngles.z * Mathf.Deg2Rad) * upSpeed, 
			Mathf.Cos (transform.rotation.eulerAngles.z * Mathf.Deg2Rad) * upSpeed);
		//GetComponent<Rigidbody2D>().MovePosition(new Vector2(transform.position.x, transform.position.y + upSpeed));
		//GetComponent<Rigidbody2D>().velocity = Vector2.up * upSpeed;
	}

	//destroy the bullet when it hits something
	void OnCollisionEnter2D(Collision2D other) {
		Debug.Log ("hit " + other.gameObject.name);

		if (other.gameObject.tag == "Top") {
			Destroy (gameObject);
		}

		if (other.gameObject.tag == "Alien") {
			other.gameObject.GetComponent<AlienBehaviour>().HandleHit();
			Destroy (gameObject);
		}
	}
}
