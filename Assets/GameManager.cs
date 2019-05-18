using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	enum PlayMode { TOUCH, ORIENTATION, MAGNETISM };
	enum MappingMode { LINEAR, ANGULAR, LINEAR_TRAINING, ANGULAR_TRAINING };

	//references to the game objects
	public Transform titleScreenPanel;
	public Text reportText;
	public Text debugText;
	public GameObject siFighter;
	public GameObject top, bottom;
	public AlienBehaviour[,] siAliens = new AlienBehaviour[3,7];
	public AlienBehaviour alienPrefab;
	public FighterBulletBehaviour fighterBulletPrefab;

	//gameplay parameters
	PlayMode playMode = PlayMode.TOUCH; //default is touch
	MappingMode mappingMode = MappingMode.LINEAR; //default is linear
	int aliensDestroyed = 0;
	int shotsFired = 0;
	float playTime = 0f; //duration of the previous gameplay
	float fighterRotationSpeed = 2f; //in angle
	float fighterLeftSpeed, fighterRightSpeed;
	float fighterSpeedRatio = 0.02f; //a mulitplier to control the fighter speed relative to the screen
	float alienDownSpeed;
	float[] alienLateralSpeed = {0.15f, -0.15f, 0.15f};
	float alienDownSpeedRatio = 0.005f; //a multipler to control the alien's speed relative to the screen
	const float stepTime = 0.2f;
	const float fighterBulletStepTime = 1f;
	float currentStepTime = stepTime; //for moving the aliens in a stepwise manner, works like a count-down timer
	float currentFighterBulletStepTime = fighterBulletStepTime; //control the auto fire from the fighter

	//the magnet stuff
	private float magnitudeThreshold = 200f; //the threshold value of magnitude to tell if the magnet is close enough
	private Vector3 magValue;
	private float angle;
	private int xAxisValueLineBufferLen = 0, yAxisValueLineBufferLen = 0, zAxisValueLineBufferLen = 0;

	//screen parameters
	float screenHeight, screenWidth;

	//flag indicating of the report is displayed (hide it from participants)
	public bool reportIsDisplayed = false;


	// Use this for initialization
	void Start () {
		screenHeight = Camera.main.orthographicSize * 2;
		screenWidth = screenHeight * Camera.main.aspect;

		//enable the magnetoeter
		Input.compass.enabled = true;	

		//request the sreen to not dim
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		//put the fighter at 10% from bottom (40% from middle)
		siFighter.transform.position = new Vector3(0f, -screenHeight*0.4f);

		//put the top (bar) to the top of the screen and scale it
		top.transform.position = new Vector3(0f, screenHeight*0.5f);
		top.transform.localScale = new Vector3 (screenWidth, 0.5f);
		//put the bottom (bar) to the bottom of the screen and scale it
		bottom.transform.position = new Vector3(0f, -screenHeight*0.5f);
		bottom.transform.localScale = new Vector3 (screenWidth, 0.5f);

		//set the left/right speed of the fighter
		fighterLeftSpeed = fighterRightSpeed = screenWidth * fighterSpeedRatio;
		//set the down speed of the aliens
		alienDownSpeed = screenHeight * alienDownSpeedRatio;

		//generate 21 aliens
		for (int i = -3; i <= 3; i++) {//clone 7 from left to right
			for (int j = 0; j < 3; j++) {//clone 3 from bottom to top
				siAliens[j,i+3] = Instantiate<AlienBehaviour>(alienPrefab, new Vector3(i*screenWidth*0.12f, j*screenHeight*0.1f+screenHeight*0.2f), Quaternion.identity);
			}
		}

		//pause the game and show the title screen
		Time.timeScale = 0f;
	}
	
	// Update is called once per frame
	void Update () {

		playTime += Time.deltaTime;

		currentStepTime -= Time.deltaTime; //update the step timer
		if (currentStepTime <= 0) {
			//update the position of the aliens
			for (int j = 0; j < 3; j++) {//move left/right
				if(siAliens[j,0].transform.position.x - Mathf.Abs(alienLateralSpeed[j]) < -screenWidth/2f || 
					siAliens[j, 6].transform.position.x + Mathf.Abs(alienLateralSpeed[j]) > screenWidth/2f) {// this alien row is about to hit the screen, turn around
					alienLateralSpeed[j] *= -1;
				}
				for (int i = -3; i <= 3; i++) {//move down
					siAliens [j, i + 3].transform.position = new Vector3 (siAliens [j, i + 3].transform.position.x + alienLateralSpeed[j], siAliens [j, i + 3].transform.position.y - alienDownSpeed);
				}
			}
			currentStepTime = stepTime; //reset the step timer
		}

		//check if it is game over (if an alien reaches the bottom of the screen, or all aliens are destroyed)
		aliensDestroyed = 0;
		//a flag to indicate if need to revserse the vertical movement of the aliens
		bool reverseDirection = false;
		for (int j = 0; j < 3; j++) {//from bottom to top
			for (int i = -3; i <= 3; i++) {//from left to right
				/*if (siAliens [j, i + 3].GetComponent<AlienBehaviour> ().isAlive && siAliens [j, i + 3].transform.position.y <= -screenHeight / 2f) {
					ShowTitleScreen ();
				}*/
				if (siAliens [j, i + 3].GetComponent<AlienBehaviour> ().isAlive && (siAliens [j, i + 3].transform.position.y <= -screenHeight*0.3f || siAliens [j, i + 3].transform.position.y >= screenHeight*0.45f)) {
					reverseDirection = true;
				}
				if (!siAliens [j, i + 3].GetComponent<AlienBehaviour> ().isAlive)
					aliensDestroyed++;
			}
		}
		if (reverseDirection) {
			if(siAliens[0, 0].transform.position.y > 0)
				alienDownSpeed = screenHeight * alienDownSpeedRatio;
			else
				alienDownSpeed = -screenHeight * alienDownSpeedRatio;
		}
		if (aliensDestroyed == 21) {//all aliens destroyed
			ShowTitleScreen();
			return;
		}

		currentFighterBulletStepTime -= Time.deltaTime;
		if (currentFighterBulletStepTime <= 0) {
			//generate a bullet from the fighter
			Instantiate<FighterBulletBehaviour> (fighterBulletPrefab, siFighter.transform.position, siFighter.transform.rotation);//Quaternion.identity);
			shotsFired++;
			currentFighterBulletStepTime = fighterBulletStepTime;
		}

		//detect input and call the corresponding function
		if (playMode == PlayMode.TOUCH) {
			if (Input.GetKey (KeyCode.LeftArrow) || (Input.touchCount>0 && Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).x<0f)) {
				HandleLeftInput ();
			}
			if (Input.GetKey (KeyCode.RightArrow) || (Input.touchCount>0 && Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position).x>0f)) {
				HandleRightInput ();
			}
		} else if (playMode == PlayMode.ORIENTATION) {
			if (Input.acceleration.x < -0.1f) {
				HandleLeftInput ();
			}
			if (Input.acceleration.x > 0.1f) {
				HandleRightInput ();
			}
		} else if (playMode == PlayMode.MAGNETISM) {
			//for the magnetic ring
			magValue = new Vector3(Input.compass.rawVector.x, Input.compass.rawVector.y, Input.compass.rawVector.z);
			angle = Mathf.Atan2 (magValue.y, magValue.x) * Mathf.Rad2Deg; //[-180, 180]

			debugText.text = "Angle = " + angle;

			if (angle < 0f) {
				HandleLeftInput ();
			}
			if (angle > 0f) {
				HandleRightInput ();
			}
		} else {//should not reach here
		}
		
	}

	// Function handling play mode
	public void ChoosePlayMode(Button b) {
		if (b.name == "Touch") {
			playMode = PlayMode.TOUCH;
			titleScreenPanel.gameObject.SetActive (false);
			ResetGame ();
		} else if (b.name == "Orientation") {
			playMode = PlayMode.ORIENTATION;
			titleScreenPanel.gameObject.SetActive (false);
			ResetGame ();
		} else if (b.name == "Magnetism") {
			playMode = PlayMode.MAGNETISM;
			titleScreenPanel.gameObject.SetActive (false);
			ResetGame ();
		} else {//should not reach here
		}
	}

	// Function handling displaying of the report
	public void ToggleReport(Button b) {
		Debug.Log ("ToggleReport");
		//toggle the displaying of the info
		if (!reportIsDisplayed) {
			//reportText.text = "Aliens\nDestroyed: " + aliensDestroyed + "\nTime: " + playTime + "s";
			reportText.text = "Shots Fired: " + shotsFired + "\nTime: " + playTime + "s";
			reportIsDisplayed = true;
		} else {
			reportText.text = "Return to researcher";
			reportIsDisplayed = false;
		}
	}

	// Function handling mapping mode
	public void ChooseMappingMode(Button b) {
		Text temp = b.transform.Find ("Text").GetComponent<Text> ();
		if (temp.text.Equals ("Linear")) {
			temp.text = "Angular";
			mappingMode = MappingMode.ANGULAR;
		} else if (temp.text.Equals ("Angular")) {
			temp.text = "Linear\nTraining";
			mappingMode = MappingMode.LINEAR_TRAINING;
		} else if (temp.text.Equals ("Linear\nTraining")) {
			temp.text = "Angular\nTraining";
			mappingMode = MappingMode.ANGULAR_TRAINING;
		} else {
			temp.text = "Linear";
			mappingMode = MappingMode.LINEAR;
		}
	}

	// Helper function to show the title screen
	void ShowTitleScreen() {
		//calculate the aliens destroyed again
		aliensDestroyed = 0;
		for (int j = 0; j < 3; j++) {//clone 3 from bottom to top
			for (int i = -3; i <= 3; i++) {//clone 7 from left to right
				if (!siAliens [j, i + 3].GetComponent<AlienBehaviour> ().isAlive)
					aliensDestroyed++;
			}
		}

		//if it is in training mode just set it to 3 (because the other 18 are automatically distroyed)
		if (mappingMode == MappingMode.ANGULAR_TRAINING || mappingMode == MappingMode.LINEAR_TRAINING) {
			aliensDestroyed = 3;
		}

		Time.timeScale = 0f; //pause the game
		titleScreenPanel.gameObject.SetActive(true);
	}

	// Helper function to reset the game
	void ResetGame() {
		//reposition the 21 aliens
		for (int i = -3; i <= 3; i++) {//7 from left to right
			for (int j = 0; j < 3; j++) {//3 from bottom to top
				siAliens[j,i+3].transform.position = new Vector3(i*screenWidth*0.12f, j*screenHeight*0.1f+screenHeight*0.2f);
				siAliens [j, i + 3].GetComponent<AlienBehaviour> ().HandleResurrect ();
			}
		}

		//kill the top 2 rows, and 2 on each end of the bottom row, if it is in training mode
		if (mappingMode == MappingMode.ANGULAR_TRAINING || mappingMode == MappingMode.LINEAR_TRAINING) {
			for (int i = -3; i <= 3; i++) {//7 from left to right
				for (int j = 1; j < 3; j++) {//from 2nd row to top
					siAliens[j,i+3].transform.position = new Vector3(i*screenWidth*0.12f, j*screenHeight*0.1f+screenHeight*0.2f);
					siAliens [j, i + 3].GetComponent<AlienBehaviour> ().HandleHit ();
				}
			}
			siAliens [0, 0].GetComponent<AlienBehaviour> ().HandleHit ();
			siAliens [0, 1].GetComponent<AlienBehaviour> ().HandleHit ();
			siAliens [0, 5].GetComponent<AlienBehaviour> ().HandleHit ();
			siAliens [0, 6].GetComponent<AlienBehaviour> ().HandleHit ();
		}

		//reset the movement direction of the aliens to downwards
		alienDownSpeed = screenHeight * alienDownSpeedRatio;

		//reposition the fighter
		siFighter.transform.position = new Vector3(0f, -screenHeight*0.4f);
		siFighter.transform.eulerAngles = new Vector3 (0f, 0f, 0f);

		//destroy all bullets
		GameObject[] gob = GameObject.FindGameObjectsWithTag("FighterBullet");
		for (int i = 0; i < gob.Length; i++) {
			Destroy (gob [i]);
		}
		shotsFired = 0;

		//make sure report info is not displayed
		reportText.text = "Return to researcher";
		reportIsDisplayed = false;

		//resume the game
		Time.timeScale = 1.0f;

		//reset gameplay duration
		playTime = 0f;

		//show debug text
		debugText.enabled = false;//true;
	}

	// Helper function handling a "left" input
	void HandleLeftInput() {
		//Debug.Log ("Left");

		if (mappingMode == MappingMode.LINEAR || mappingMode == MappingMode.LINEAR_TRAINING) {
			if (playMode == PlayMode.MAGNETISM) {
				siFighter.transform.position = new Vector3 (Mathf.Lerp(-screenWidth / 2, 0f, (Mathf.Clamp(angle, -90f, 0f)+90f)/90f), siFighter.transform.position.y);
			} else {
				if (siFighter.transform.position.x - fighterLeftSpeed < -screenWidth / 2) {
					//if goes out of screen, return
					return;
				}
				siFighter.transform.position = new Vector3 (siFighter.transform.position.x - fighterLeftSpeed, siFighter.transform.position.y);
			}
		} else {//angular mapping
			if (playMode == PlayMode.MAGNETISM) {
				//siFighter.transform.eulerAngles = new Vector3(0f, 0f, -Mathf.Clamp (angle, -90f, 0f));
				siFighter.transform.eulerAngles = new Vector3(0f, 0f, -Mathf.Clamp (angle, -70f, 0f)); //limit the angle to the 70-degree to the left
			} else {
				//if (90 < siFighter.transform.eulerAngles.z + fighterRotationSpeed && siFighter.transform.eulerAngles.z + fighterRotationSpeed < 270) {
				if (70 < siFighter.transform.eulerAngles.z + fighterRotationSpeed && siFighter.transform.eulerAngles.z + fighterRotationSpeed < 290) {
					//if rotates more than 70 degrees, return
					return;
				}
				siFighter.transform.eulerAngles = new Vector3 (0f, 0f, siFighter.transform.eulerAngles.z + fighterRotationSpeed);
			}
		}
	}

	// Helper function handling a "right" input
	void HandleRightInput() {
		//Debug.Log ("right");
		if (mappingMode == MappingMode.LINEAR || mappingMode == MappingMode.LINEAR_TRAINING) {
			if (playMode == PlayMode.MAGNETISM) {
				siFighter.transform.position = new Vector3 (Mathf.Lerp (0f, screenWidth / 2, (Mathf.Clamp (angle, 0f, 90f) / 90f)), siFighter.transform.position.y);
			} else {
				if (siFighter.transform.position.x + fighterRightSpeed > screenWidth / 2) {
					//if goes out of screen, return
					return;
				}
				siFighter.transform.position = new Vector3 (siFighter.transform.position.x + fighterRightSpeed, siFighter.transform.position.y);
			}
		} else {
			if (playMode == PlayMode.MAGNETISM) {
				//siFighter.transform.eulerAngles = new Vector3 (0f, 0f, 360f-Mathf.Clamp (angle, 0f, 90f));
				siFighter.transform.eulerAngles = new Vector3 (0f, 0f, 360f-Mathf.Clamp (angle, 0f, 70f)); //limit the angle to the 70-degree to the right
			} else {
				//if (90 < siFighter.transform.eulerAngles.z - fighterRotationSpeed && siFighter.transform.eulerAngles.z - fighterRotationSpeed < 270) {
					if (70 < siFighter.transform.eulerAngles.z - fighterRotationSpeed && siFighter.transform.eulerAngles.z - fighterRotationSpeed < 290) {
					//if rotates more than 70 degrees, return
					return;
				}
				siFighter.transform.eulerAngles = new Vector3 (0f, 0f, siFighter.transform.eulerAngles.z - fighterRotationSpeed);
			}
		}
	}
}
