using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {
	
	public GameObject[] levelSelectButtons = new GameObject[9];
	public GameObject mainMenuCanvas;
	public GameObject levelSelectCanvas;
	public GameObject continueButton;

	void Start () {

        //System.IO.File.WriteAllText("User Levels\\Beginner Buttons.lv",Crypto.Encrypt(System.IO.File.ReadAllText("User Levels\\Beginner Buttons.lv")));
        //System.IO.File.WriteAllText("User Levels\\Beginner Buttons.lv", Crypto.Decrypt(System.IO.File.ReadAllText("User Levels\\aaa.lv")));
        Debug.Log(PlayerPrefs.GetInt("currentLevel").ToString());

        GameObject.Find("Level Select Button").GetComponent<Button>().interactable = false;
		if(GameData.initialized == false)
			GameData.Initialize();

		if(!PlayerPrefs.HasKey("currentLevel"))
		{
			PlayerPrefs.SetInt("currentLevel", 0);
		}

		if(PlayerPrefs.GetInt("currentLevel") > 8 || PlayerPrefs.GetInt("currentLevel") < 0)
			PlayerPrefs.SetInt("currentLevel", 0);

		if(PlayerPrefs.GetInt("currentLevel") == 0)
			continueButton.GetComponent<Button>().interactable = false;

		for(int x = 0; x <= PlayerPrefs.GetInt("currentLevel"); x++)
		{
			levelSelectButtons[x].GetComponent<Button>().interactable = true;
		}
	}

	public void ShowLevelSelect()
	{
		mainMenuCanvas.GetComponent<Canvas>().enabled = false;
		levelSelectCanvas.GetComponent<Canvas>().enabled = true;
	}

	public void ShowMainMenu()
	{
		mainMenuCanvas.GetComponent<Canvas> ().enabled = true;
		levelSelectCanvas.GetComponent<Canvas> ().enabled = false;
	}

	public void StartGameAtLevel(int level)
	{
		LevelLoader.levelToLoad = level;
		Application.LoadLevel(1);
	}

	public void EnterLevelEditor()
	{
		Application.LoadLevel(3);
	}

	public void StartNewGame()
	{
		PlayerPrefs.SetInt("currentLevel", 0);
		LevelLoader.levelToLoad = 0;
		Application.LoadLevel(1);
	}

	public void ContinueGame()
	{
		LevelLoader.levelToLoad = PlayerPrefs.GetInt("currentLevel");
		Application.LoadLevel(1);
	}
}