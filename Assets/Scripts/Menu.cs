using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

    public GameObject[] worldPanels = new GameObject[3];
	public GameObject[] levelSelectButtons = new GameObject[9];
	public GameObject mainMenuPanel;
	public GameObject worldSelectPanel;
    public GameObject whatsNewPanel;
    public GameObject creditsPanel;
    public GameObject selectorPanel;
    public GameObject nextWorldButton;
    public GameObject previousWorldButton;
    public GameObject worldNumberText;
    public GameObject worldNameText;
    public GameObject optionsButton;

    private GameObject currentWorldPanel;
    private int currentWorld;
    private string[] worlds = new string[3];
    private string[] worldNames = new string[3];

	void Start () {
        //Worlds are hardcoded for now, will be determined from file in future
        worlds = new string[]{ "World 1", "World 2", "User Levels" };
        worldNames = new string[] { "\"The Basics\"", "\"Lasers 'n Stuff\"", "Extremely Basic/Buggy" };
        currentWorldPanel = worldPanels[0];
        currentWorld = 0;

        //Options not yet implemented, disable button until they are
        optionsButton.GetComponent<Button>().interactable = false;

		if(GameData.initialized == false)
			GameData.Initialize();

		if(!PlayerPrefs.HasKey("currentLevel"))
		{
			PlayerPrefs.SetInt("currentLevel", 0);
		}

		if(PlayerPrefs.GetInt("currentLevel") > 8 || PlayerPrefs.GetInt("currentLevel") < 0)
			PlayerPrefs.SetInt("currentLevel", 0);

		for(int x = 0; x <= PlayerPrefs.GetInt("currentLevel"); x++)
		{
			levelSelectButtons[x].GetComponent<Button>().interactable = true;
		}
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

    public void EnterWorldSelect()
    {
        worldSelectPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        SelectWorld(0);
    }

    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void ShowWhatsNew()
    {
        mainMenuPanel.SetActive(false);
        whatsNewPanel.SetActive(true);
    }

    public void ReturnToMenu()
    {
        worldSelectPanel.SetActive(false);
        whatsNewPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void SelectWorld(int i)
    {
        currentWorld = i;
        worldNumberText.GetComponent<Text>().text = worlds[i];
        worldNameText.GetComponent<Text>().text = worldNames[i];

        if (i == 0)
            previousWorldButton.SetActive(false);
        else
            previousWorldButton.SetActive(true);

        if (i == worlds.Length - 1)
            nextWorldButton.SetActive(false);
        else
            nextWorldButton.SetActive(true);

        currentWorldPanel.SetActive(false);
        currentWorldPanel = worldPanels[i];
        currentWorldPanel.SetActive(true);
    }

    public void PreviousWorld()
    {
        SelectWorld(currentWorld - 1);
    }

    public void NextWorld()
    {
        SelectWorld(currentWorld + 1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}