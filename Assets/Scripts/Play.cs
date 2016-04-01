using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

sealed public class Play : MonoBehaviour {

	static int[][] directions = new int[][]{new int[]{0,1}, new int[]{1,0}, new int[]{0,-1}, new int[]{-1,0}};

	public GameObject GameplayCanvas;
	public GameObject PauseCanvas;
	public GameObject ErrorCanvas;

	public bool levelWon;
	public Text timeText;
	public Text levelText;
	public Text errorText;

	private Level currentLevel;
	private bool levelLoaded;
	private Entity player;
	private bool inputReady;
	private bool gamePaused;
	private bool errorOccured;
	private float levelTimer;
	private float cameraZoom = 35f;
    private byte clockCycle;
    private float clockTimer;
    private bool moved;
    private int[] dir = new int[] { 0, 0 };

    void Start()
    {
		player = GameObject.Find("Player").GetComponent<Entity>();
		Camera.main.transform.position = player.transform.position + (Camera.main.transform.position - player.transform.position).normalized * cameraZoom;
        if (LevelLoader.levelToLoad == -1)
            StartLevel(LevelLoader.JSONToLoad);
        else
            StartLevel(LevelLoader.levelToLoad);
	}

	void Update () {
		if(!levelLoaded || gamePaused || errorOccured)
			return;

		levelTimer += Time.deltaTime;
		timeText.text = Mathf.FloorToInt(levelTimer / 60F).ToString("00") + ":" + Mathf.FloorToInt(levelTimer % 60).ToString("00");

		if(levelWon || currentLevel.LevelID == -1 && Input.GetKeyDown(KeyCode.Escape))
		{
			levelLoaded = false;
			levelWon = false;
			inputReady = false;
			currentLevel.EndLevel();
            if (currentLevel.LevelID == -1)
            {
                LevelEditor.loadingLevel = true;
                LevelEditor.loadingWonLevel = levelWon;
				StartCoroutine(StartLevelWait(-1));
            }
            else
            {
                StartCoroutine(StartLevelWait(currentLevel.LevelID + 1));
            }
		}

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            dir = GetMovementDirection(KeyCode.W);
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            dir = GetMovementDirection(KeyCode.S);
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dir = GetMovementDirection(KeyCode.A);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dir = GetMovementDirection(KeyCode.D);
        }
        else if (moved)
        {
            dir = new int[] { 0, 0 };
            moved = false;
        }

        if (Time.time - clockTimer >= 0.05f && !player.IsMoving && inputReady)
        {
            if (dir[0] != 0 || dir[1] != 0)
            {
                currentLevel.MoveEntity(player, dir, true, clockCycle);
                StartCoroutine(InputCooldown(0.3f));
                moved = true;
            }
        }

        if (Input.GetKey(KeyCode.Q))
			Camera.main.transform.RotateAround(player.transform.position, Vector3.up, 60f * Time.deltaTime);
		if(Input.GetKey(KeyCode.E))
			Camera.main.transform.RotateAround(player.transform.position, Vector3.up, -60f * Time.deltaTime);
		if(Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
			cameraZoom = Mathf.Clamp(cameraZoom - (60 * Time.deltaTime), 15f, 35f);
		else if(Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
			cameraZoom = Mathf.Clamp(cameraZoom + (60 * Time.deltaTime), 15f, 35f);

		if(Input.GetKey(KeyCode.R))
			RestartLevel();
		if(Input.GetKey(KeyCode.P))
			Pause();

		Camera.main.transform.position = player.transform.position + (Camera.main.transform.position - player.transform.position).normalized * cameraZoom;

        //Utilise a clock cycle based system to syncronise movement between objects
        if (Time.time - clockTimer > 0.05f)
        {
            if (clockCycle != 6)
                clockCycle++;
            else
                clockCycle = 0;

            clockTimer = Time.time;
        }
    }

    //Called when starting a level used for testing in the level editor
    private IEnumerator StartLevelWait()
    {
        yield return new WaitForSeconds(1.25f);
        StartLevel(LevelLoader.JSONToLoad);        
    }

    //Called when starting a level outsie of the level editor
    private IEnumerator StartLevelWait(int ID)
	{
		yield return new WaitForSeconds(1.25f);

		if(ID >= 9)
			Application.LoadLevel(2);
		else if(ID == -1)
			Application.LoadLevel(3);
		else
		{
			if(PlayerPrefs.GetInt("currentLevel") < ID)
				PlayerPrefs.SetInt("currentLevel", ID);

			StartLevel(ID);
		}
	}

	private void StartLevel(int ID)
	{
		try
		{
			currentLevel = LevelLoader.Load("LevelData", ID);
            currentLevel.InputCooldown = Time.time;
			levelLoaded = true;
			StartCoroutine(InputCooldown(1f));
			levelTimer = 0;
            levelText.text = "Level " + (currentLevel.LevelID + 1).ToString();
		}
		catch(Exception ex)
		{
			if(ex is System.IO.FileNotFoundException)
				Debug.Log("Level File Missing");
			else
				Debug.Log(ex.ToString());

			Error(ex.ToString());
		}
	}

    private void StartLevel(Dictionary<string, object> rawJSON)
    {
        try
        {
            currentLevel = LevelLoader.LoadFromJSON(rawJSON,-1);
            levelLoaded = true;
            StartCoroutine(InputCooldown(1f));
            levelTimer = 0;
            levelText.text = "Level " + (currentLevel.LevelID + 1).ToString();
        }
        catch (Exception ex)
        {
            if (ex is System.IO.FileNotFoundException)
                Debug.Log("Level File Missing");
            else
                Debug.Log(ex.ToString());

            Error(ex.ToString());
        }
    }

    public void RestartLevel()
	{
		if(!player.IsMoving && inputReady && levelLoaded)
		{
			levelLoaded = false;
			inputReady = false;
			currentLevel.EndLevel();
            if (currentLevel.LevelID == -1)
                StartCoroutine(StartLevelWait());
            else
			    StartCoroutine(StartLevelWait(currentLevel.LevelID));
		}
	}

	public void Pause()
	{
		if(!player.IsMoving && inputReady && levelLoaded)
		{
			GameplayCanvas.GetComponent<Canvas>().enabled = false;
			PauseCanvas.GetComponent<Canvas>().enabled = true;
			gamePaused = true;
		}
	}

	private void Error(string errorMessage)
	{
		GameplayCanvas.GetComponent<Canvas>().enabled = false;
		PauseCanvas.GetComponent<Canvas>().enabled = false;
		ErrorCanvas.GetComponent<Canvas>().enabled = true;
		errorText.text = "Error: " + errorMessage;
		errorOccured = true;
	}

	public void ErrorButtonClicked()
	{
		Application.LoadLevel(0);
	}

	public void UnPause()
	{
		GameplayCanvas.GetComponent<Canvas>().enabled = true;
		PauseCanvas.GetComponent<Canvas>().enabled = false;
		gamePaused = false;
	}

	public void LeaveGame()
	{
		Application.LoadLevel(0);
	}

	IEnumerator InputCooldown(float time)
	{
		inputReady = false;
		yield return new WaitForSeconds(time);
		inputReady = true;
        currentLevel.InputCooldown = 0f;
	}

    //Get direction to move player based off the camera angle when a movement key was input
	int[] GetMovementDirection(KeyCode key)
	{
		int[] dir = directions[0];
		float cameraRot = Camera.main.transform.rotation.eulerAngles.y;
		int angleDirection = 0;

		if(cameraRot >= 315 || cameraRot < 45)
			angleDirection = 0;
		else if(cameraRot >= 45 && cameraRot < 135)
			angleDirection = 1;
		else if(cameraRot >= 135 && cameraRot < 225)
			angleDirection = 2;
		else if(cameraRot >= 225 && cameraRot < 315)
			angleDirection = 3;

		if(key == KeyCode.W)
			dir = directions[angleDirection];
		if(key == KeyCode.D)
			dir = directions[(angleDirection + 1) % 4];
		if(key == KeyCode.S)
			dir = directions[(angleDirection + 2) % 4];
		if(key == KeyCode.A)
			dir = directions[(angleDirection + 3) % 4];

		return dir;
	}
}