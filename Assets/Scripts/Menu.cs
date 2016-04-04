using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour {

    public static bool returningFromUserLevel;
    public static int returnPage;

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

    //User Levels Objects
    public int[] userLevelIDs;
    public GameObject[] userLevelButtons = new GameObject[6];
    public GameObject PageText;
    public GameObject previousPageButton;
    public GameObject nextPageButton;
    public GameObject refreshPageButton;
    public GameObject userLevelMessageText;
    private int currentPage;

    private GameObject currentWorldPanel;
    private int currentWorld;
    private string[] worlds = new string[3];
    private string[] worldNames = new string[3];

	void Start () {
        //Worlds are hardcoded for now, will be determined from file in future
        worlds = new string[]{ "World 1", "World 2", "User Levels" };
        worldNames = new string[] { "\"The Basics\"", "\"Lasers 'n Stuff\"", "Extremely Basic/Buggy" };
        previousPageButton.GetComponent<Button>().interactable = false;
        currentWorldPanel = worldPanels[0];
        currentWorld = 0;

        if (returningFromUserLevel)
        {
            mainMenuPanel.SetActive(false);
            worldSelectPanel.SetActive(true);
            SelectWorld(2);
            StartCoroutine(GetUserLevels(returnPage));
            returningFromUserLevel = false;
        }

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
		Application.LoadLevel(2);
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
        {
            nextWorldButton.SetActive(false);
            StartCoroutine(GetUserLevels(0));
        }
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

    private IEnumerator GetUserLevels(int page)
    {
        WWWForm form = new WWWForm();

        form.AddField("page", page.ToString());

        //WWW w = new WWW("127.0.0.1/getLevels.php", form);
        WWW w = new WWW("michael-walsh.co.uk/getLevels.php", form);
        userLevelMessageText.SetActive(true);
        userLevelMessageText.GetComponent<Text>().text = "Loading Page...";

        yield return w;

        if (w.error != null || w.text[0] == '£')
        {
            for(int i = 0; i < userLevelButtons.Length; i++)
                userLevelButtons[i].SetActive(false);
            userLevelMessageText.GetComponent<Text>().text = w.error;
            previousPageButton.GetComponent<Button>().interactable = true;
            nextPageButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            string[] levelInfo = w.text.Split('@');
            userLevelIDs = new int[levelInfo.Length];
            userLevelMessageText.SetActive(false);

            for(int i = 0; i < userLevelButtons.Length; i++)
            {
                if (i < levelInfo.Length)
                {
                    userLevelButtons[i].SetActive(true);
                    userLevelIDs[i] = int.Parse(levelInfo[i].Split('#')[0]);
                    userLevelButtons[i].transform.GetChild(0).GetComponent<Text>().text = levelInfo[i].Split('#')[1];
                }
                else
                    userLevelButtons[i].SetActive(false);
            }

            nextPageButton.GetComponent<Button>().interactable = levelInfo.Length == 7? true : false;
            previousPageButton.GetComponent<Button>().interactable = page == 0? false : true;
            PageText.GetComponent<Text>().text = "Page: " + (page + 1).ToString();
            currentPage = page;
        }
    }

    public void LoadPreviousPage()
    {
        StartCoroutine(GetUserLevels(currentPage - 1));
    }

    public void LoadNextPage()
    {
        StartCoroutine(GetUserLevels(currentPage + 1));
    }

    public void RefreshPage()
    {
        StartCoroutine(GetUserLevels(currentPage));
    }

    public void StartLoadPlayerLevel(int id)
    {
        StartCoroutine(LoadPlayerLevel(id));
    }

    private IEnumerator LoadPlayerLevel(int id)
    {
        WWWForm form = new WWWForm();

        form.AddField("LevelID", userLevelIDs[id].ToString());

        //WWW w = new WWW("127.0.0.1/downloadLevel.php", form);
        WWW w = new WWW("michael-walsh.co.uk/downloadLevel.php", form);

        userLevelMessageText.SetActive(true);
        userLevelMessageText.GetComponent<Text>().text = "Loading Level...";
        for (int i = 0; i < userLevelButtons.Length; i++)
            userLevelButtons[i].SetActive(false);

        yield return w;

        if (w.error != null || w.text[0] == '£')
        {
            for (int i = 0; i < userLevelButtons.Length; i++)
                userLevelButtons[i].SetActive(false);
            userLevelMessageText.SetActive(true);
            userLevelMessageText.GetComponent<Text>().text = w.error;
            previousPageButton.SetActive(false);
            nextPageButton.SetActive(false);
        }
        else
        {
            LevelLoader.JSONToLoad = MiniJSON.Json.Deserialize(w.text) as Dictionary<string, object>;
            LevelLoader.levelToLoad = -2;
            returnPage = currentPage;
            Application.LoadLevel(1);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}