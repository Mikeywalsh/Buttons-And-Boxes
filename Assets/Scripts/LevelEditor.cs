using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

sealed public class LevelEditor : MonoBehaviour {
    public static bool loadingLevel;
    public static bool loadingWonLevel;
    public static Texture2D testedLevelImage;

    //Level Editor Objects
	public GameObject levelObjects;
	public GameObject widthArrows;
	public GameObject lengthArrows;
    public GameObject selectionPanel;
    public GameObject settingsPanel;
    public GameObject openLevelPanel;
    public GameObject tinkerPanel;
    public GameObject uploadPanel;
    public GameObject exitPanel;
    public GameObject tooltipPanel;
    public GameObject screenshotPanel;
    public GameObject messagePanel;
    public GameObject messageText;
    public GameObject notificationText;
    public GameObject errorText;
    public GameObject tinkerStartDropdown;
    public GameObject playButton;
    public GameObject uploadButton;

    //Level Settings Objects
    public Transform levelInformation;
    public Slider RedInput;
    public Slider GreenInput;
    public Slider BlueInput;
    public Sprite Panel;
    public Sprite SelectedPanel;
    public Sprite questionMark;
    public string levelName;
    public string levelDifficulty;

    private int playerX;
    private int playerY;
	private string sessionID;
    private Color32 levelColor;
	private RaycastHit hit;	
	private GameObject cursor;
	private GameObject currentObject;
	private GameObject currentArrow;
    private GameObject activePanel;
    private Transform currentButton;
    private Sprite levelSprite;
    private Sprite lastLevelSprite;
	private Plane ground = new Plane(Vector3.up, Vector3.zero);
	private Vector3 mouseVector;
	private float hitDist;
	private bool validChoice;
	private bool cursorOOB;
    private bool levelComplete;
    private bool levelChanged;
    private bool minorChange;
    private bool helpEnabled;

	private int currentWidth;
	private int currentLength;

	private char selectedLayer;
	private char selectedBlock;
    private Mechanism selectedMechanism;
    private GameObject selectedMechObject;

	private char?[,] groundLayout = new char?[30,30];
	private char?[,] entityLayout = new char?[30,30];
	private char?[,] mechanismLayout = new char?[30,30];
	private Mechanism[,] mechanisms = new Mechanism[30,30];

	void Start () {
        sessionID = "";
        playerX = -1;
        levelSprite = questionMark;
        lastLevelSprite = questionMark;

        if(!GameData.initialized)
            GameData.Initialize();

        SelectionSwitched("GF#Floor");
        activePanel = settingsPanel;
        levelColor = new Color32(162, 210, 190, 255);
        levelDifficulty = "Easy";

        if (loadingLevel)
        {
            LoadLevel(LevelLoader.JSONToLoad);
            levelComplete = loadingWonLevel;
            loadingWonLevel = false;
            loadingLevel = false;
        }
        else
        {
            currentWidth = 13;
            currentLength = 13;
            Camera.main.GetComponent<CameraControl>().SetPivotPoint(new Vector3(currentWidth - 1, 0, currentLength - 1));
        }
	}

	void Update () {
        if (!selectionPanel.activeSelf)
            return;
         
        if(tooltipPanel.activeSelf)
            tooltipPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x + (GameObject.Find("Tooltip Panel").GetComponent<RectTransform>().rect.width / 2), Input.mousePosition.y - Screen.height - (GameObject.Find("Tooltip Panel").GetComponent<RectTransform>().rect.height / 2));

        if (levelComplete && levelChanged)
            levelComplete = false;

		if(Input.GetKeyDown(KeyCode.U))
			StartCoroutine(GetUserFromSession());
        
		//Reset Selected arrow each frame
		if(currentArrow)
			currentArrow.GetComponent<Renderer>().material.color = Color.white;

		#region Allow User To Clear Entire Level
		//if(Input.GetKeyDown(KeyCode.Backspace))
		//{
        //    ClearLevel();
		//}
		#endregion

		Ray mouseRay = Camera.main.ScreenPointToRay (Input.mousePosition);

        //Destroy the cursor this frame if one is present, so the next can be spawned
        if (cursor)
            Destroy(cursor);

        //Check if mouse is being hovered over valid level space and allow to user to create or destroy aspects of the level
        if (ground.Raycast(mouseRay, out hitDist) && ValidMousePos() && !helpEnabled)
		{
			#region Create Appropriate Cursor If Mouse Is Within Bounds Of Level
			mouseVector = mouseRay.GetPoint(hitDist);
			mouseVector = new Vector3(Mathf.Round(mouseRay.GetPoint(hitDist).x / 2) * 2,0,Mathf.Round(mouseRay.GetPoint(hitDist).z / 2) * 2);

			if((int)(mouseVector.x / 2) >= 0 && (int)(mouseVector.z / 2) >= 0 && (int)(mouseVector.x / 2) < currentWidth && (int)(mouseVector.z / 2) < currentLength)
			{
				cursorOOB = false;
				if(selectedLayer == 'G' && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null ||
                   selectedLayer == 'E' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'P' && mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'X' ||
                   selectedLayer == 'M' && selectedBlock != 'T' && mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'P' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == null && groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != 'X' ||
                   selectedLayer == 'M' && selectedBlock == 'T' && mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null)
				{
					validChoice = true;
				}
				else
				{
					validChoice = false;
				}
			}
			else
			{
				cursorOOB = true;
			}

			if(!cursorOOB)
			{
				if(validChoice)
				{
					cursor = Instantiate(Resources.Load("ValidChoice"), (selectedLayer == 'G') ? mouseVector - new Vector3(0,1,0) : mouseVector + new Vector3(0,1,0), Quaternion.identity) as GameObject;
				}
				else
				{
					cursor = Instantiate(Resources.Load("InvalidChoice"), (selectedLayer == 'G') ? mouseVector - new Vector3(0,1,0) : mouseVector + new Vector3(0,1,0), Quaternion.identity) as GameObject;
				}
			}
			#endregion

			#region Instantiate Or Destroy A Level Object Based On Users Input
			if(Input.GetMouseButton(0) && validChoice && !cursorOOB)
			{
				if(selectedLayer == 'G')
				{
					currentObject = Instantiate(GameData.GroundTypes[selectedBlock], mouseVector - new Vector3(0, 1, 0), Quaternion.identity) as GameObject;
					groundLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = selectedBlock;
                    levelChanged = true;
                }
				else if(selectedLayer == 'E')
				{
					currentObject = Instantiate(GameData.EntityTypes[selectedBlock], mouseVector + new Vector3(0, 1, 0), Quaternion.identity) as GameObject;
					entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = selectedBlock;
                    levelChanged = true;

                    if (selectedBlock == 'P')
                    {
                        if(playerX != -1)
                            StartCoroutine(DeleteBlock('E', playerX, playerY));
                        playerX = (int)(mouseVector.x / 2);
                        playerY = (int)(mouseVector.z / 2);
                    }
				}
				else if(selectedLayer == 'M')
				{
                    //If tinker is not selected, spawn in selected mechanism as usual
                    if (selectedBlock != 'T')
                    {
                        currentObject = Instantiate(GameData.MechanismTypes[selectedBlock], mouseVector + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(-90, 0, 0))) as GameObject;
                        mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = selectedBlock;
                        mechanisms[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] = currentObject.GetComponent<Mechanism>();
                        levelChanged = true;
                    }
                    else
                    {
                        Destroy(selectedMechObject);
                        selectedMechObject = Instantiate(Resources.Load("SelectedMechanism"), mouseVector + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(-90, 0, 0))) as GameObject;
                        selectedMechanism = mechanisms[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)];
                        OpenTinker();
                    }
                }

                if (!(selectedLayer == 'M' && selectedBlock == 'T'))
                {
                    currentObject.transform.parent = levelObjects.transform;
                    currentObject.name = selectedLayer + ((int)(mouseVector.x / 2)).ToString("00") + ((int)(mouseVector.z / 2)).ToString("00");
                    currentObject.GetComponent<Block>().Spawn(0, selectedBlock);
                }
			}
			else if(Input.GetMouseButton(1) && !validChoice && !cursorOOB)
			{
				//Stop the user from deleting ground if there is another object resting on it
				if(selectedLayer == 'G' && (entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null || mechanismLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null))
					return;

                //Stop the user from deleting mechanisms if they have selected an entity
                if(selectedLayer == 'E' && mechanismLayout[(int)(mouseVector.x / 2),(int)(mouseVector.z / 2)] != null)
                    return;

                //Stop the user from deleting entities if they have selected a mechanism
                if (selectedLayer == 'M' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] != null)
                    return;

                //If deleting a player entity, reset the player position variable
                if (selectedLayer == 'E' && entityLayout[(int)(mouseVector.x / 2), (int)(mouseVector.z / 2)] == 'P')
                    playerX = -1;

                StartCoroutine(DeleteBlock(selectedLayer, (int)(mouseVector.x / 2), (int)(mouseVector.z / 2)));
			}
			#endregion
		}

		#region Level Size Control Via Arrows
		//Check if user is hovering cursor over the resizable level arrows
		if(Physics.Raycast(mouseRay, out hit) && hit.collider.gameObject.tag == "EditorArrow" && ValidMousePos())
		{
			currentArrow = hit.collider.gameObject;
			currentArrow.GetComponent<Renderer>().material.color = Color.yellow;

			//Resize level depending on which arrow clicked
			if(Input.GetMouseButtonDown(0))
			{
				int sizeModifier = (currentArrow.name.Contains("Increase")) ? 1 : -1;
				if(currentArrow.transform.parent.gameObject == lengthArrows && (sizeModifier == 1 && currentLength < 29 || sizeModifier == -1 && currentLength > 4))
				{
					currentLength += sizeModifier;
					iTween.MoveTo(lengthArrows, iTween.Hash("position", new Vector3(currentWidth - 3,0,(currentLength * 2) + 1), "time", .5f));
					iTween.MoveTo(widthArrows, iTween.Hash("position", new Vector3((currentWidth * 2) + 1,0,currentLength + 1), "time", .5f));
					Camera.main.GetComponent<Grid>().ChangeGridSize(false, sizeModifier * 2);

					if(sizeModifier == -1)
						ClearLine(true, currentLength, currentWidth);

					Camera.main.GetComponent<CameraControl>().ChangePivotPoint(new Vector3(0,0, sizeModifier));
					CheckArrows();
				}
				else if(currentArrow.transform.parent.gameObject == widthArrows && (sizeModifier == 1 && currentWidth < 29 || sizeModifier == -1 && currentWidth > 4))
				{
					currentWidth += sizeModifier;
					iTween.MoveTo(lengthArrows, iTween.Hash("position", new Vector3(currentWidth - 3,0,(currentLength * 2) + 1), "time", .5f));
					iTween.MoveTo(widthArrows, iTween.Hash("position", new Vector3((currentWidth * 2) + 1,0,currentLength + 1), "time", .5f));
					Camera.main.GetComponent<Grid>().ChangeGridSize(true, sizeModifier * 2);

					if(sizeModifier == -1)
						ClearLine(false, currentWidth, currentLength);

					Camera.main.GetComponent<CameraControl>().ChangePivotPoint(new Vector3(sizeModifier,0, 0));
					CheckArrows();
				}
                levelChanged = true;
            }
        }
		#endregion
	}

	void ClearLine(bool row, int number, int amount)
	{
		#region Clear An Entire Row Or Column If Reducing Level Size
		for(int i = 0; i <= amount ; i++)
		{
			if(groundLayout[row? i : number, row? number : i] != null)
				StartCoroutine(DeleteBlock('G', row? i : number, row? number : i));
			if(entityLayout[row? i : number, row? number : i] != null)
				StartCoroutine(DeleteBlock('E', row? i : number, row? number : i));
			if(mechanismLayout[row? i : number, row? number : i] != null)
				StartCoroutine(DeleteBlock('M', row? i : number, row? number : i));
		}
		#endregion
	}

	IEnumerator DeleteBlock(char layer, int x, int y)
	{
        #region Delete Specific Block Found Using Coordinates

		if(levelObjects.transform.FindChild(layer + x.ToString("00") + y.ToString("00")))
		{
            if (levelObjects.transform.FindChild(layer + x.ToString("00") + y.ToString("00")).GetComponent<Block>().Despawning)
                yield break;

            levelChanged = true;
            levelObjects.transform.FindChild(layer + x.ToString("00") + y.ToString("00")).GetComponent<Block>().Despawn();
			yield return new WaitForSeconds(0.5f);
			if(layer == 'G')
				groundLayout[x,y] = null;
			else if(layer == 'E')
				entityLayout[x,y] = null;
			else if(layer == 'M')
				mechanismLayout[x,y] = null;
		}
		else
		{
			Debug.Log("No block found at: " + layer + x.ToString("00") + y.ToString("00"));
		}
		#endregion
	}

    private void ClearLevel()
    {
        ClearTinker();
        playerX = -1;
        StartCoroutine(DisablePlayUpload());

        for (int x = 0; x < currentWidth; x++)
        {
            for (int y = 0; y < currentLength; y++)
            {
                if (groundLayout[x, y] != null)
                    StartCoroutine(DeleteBlock('G', x, y));
                if (entityLayout[x, y] != null)
                    StartCoroutine(DeleteBlock('E', x, y));
                if (mechanismLayout[x, y] != null)
                    StartCoroutine(DeleteBlock('M', x, y));
            }
        }
    }

    private IEnumerator DisablePlayUpload()
    {
        playButton.GetComponent<Button>().interactable = false;
        uploadButton.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(2.5f);
        playButton.GetComponent<Button>().interactable = true;
        uploadButton.GetComponent<Button>().interactable = true;
    }

    private void ClearTinker()
    {
        if (selectedMechObject)
            Destroy(selectedMechObject);

        if (tinkerPanel.activeSelf)
            tinkerPanel.SetActive(false);
    }

	void CheckArrows()
	{
		#region Arrow Enabling/Disabling based on level size
		//Width too low
		if(currentWidth <= 4)
			widthArrows.transform.FindChild("Width Decrease").gameObject.SetActive(false);
		else if(!widthArrows.transform.FindChild("Width Decrease").gameObject.activeSelf)
			widthArrows.transform.FindChild("Width Decrease").gameObject.SetActive(true);
		
		//Width too high
		if(currentWidth >= 29)
			widthArrows.transform.FindChild("Width Increase").gameObject.SetActive(false);
		else if(!widthArrows.transform.FindChild("Width Increase").gameObject.activeSelf)
			widthArrows.transform.FindChild("Width Increase").gameObject.SetActive(true);
		
		//Length too low
		if(currentLength <= 4)
			lengthArrows.transform.FindChild("Length Decrease").gameObject.SetActive(false);
		else if(!lengthArrows.transform.FindChild("Length Decrease").gameObject.activeSelf)
			lengthArrows.transform.FindChild("Length Decrease").gameObject.SetActive(true);
		
		//Length too high
		if(currentLength >= 29)
			lengthArrows.transform.FindChild("Length Increase").gameObject.SetActive(false);
		else if(!lengthArrows.transform.FindChild("Length Increase").gameObject.activeSelf)
			lengthArrows.transform.FindChild("Length Increase").gameObject.SetActive(true);
		#endregion
	}

	public void SelectionSwitched(string s)
	{
        //Split s into array, one containing selected layer and block characters, and one containing the button name
        string[] contents = s.Split('#');

        if (contents.Length != 2 || contents[0].Length != 2)
			return;

        //Clean up gameobjects left over from previous mechanism tinkering
        if (selectedMechObject != null)
            Destroy(selectedMechObject);
        if (tinkerPanel.activeSelf)
            tinkerPanel.SetActive(false);

        if (contents[1] == "Help")
        {
            helpEnabled = true;
            ShowTooltip("Help");
        }
        else
        {
            helpEnabled = false;
            HideTooltip();
            selectedLayer = contents[0][0];
            selectedBlock = contents[0][1];
        }

        if (currentButton)
            currentButton.GetComponent<Image>().sprite = Panel;
        currentButton = GameObject.Find(contents[1] + " Button").transform;
        currentButton.GetComponent<Image>().sprite = SelectedPanel;
    }

    Dictionary<string, object> levelAsJSON()
	{
		Dictionary<string, object> levelData = new Dictionary<string, object>();

		string groundLayer = "";
		string entityLayer = "";
		string mechanismLayer = "";
        string mechanismsText = "";

		int minX = -1;
		int maxX = currentWidth;
		int minY = -1;
		int maxY = currentLength;

		#region Determine Actual Level Dimensions By Removing Rows/Columns Of Free Space
		for(int x = 0; x < currentWidth; x++)
		{
			for(int y = 0; y < currentLength; y++)
			{
				if(groundLayout[x,y] != null && entityLayout != null && mechanismLayout != null)
				{
					if(minX == -1)
						minX = x;
					maxX = x;
					break;
				}
			}
		}

        //TEMP ALPHA V0.1.0 PLEASE DELETE
        if (minX == -1)
            return levelData;
        //----

        for (int y = 0; y < currentLength; y++)
		{
			for(int x = 0; x < currentWidth; x++)
			{
				if(groundLayout[x,y] != null && entityLayout != null && mechanismLayout != null)
				{
					if(minY == -1)
						minY = y;
					maxY = y;
					break;
				}
			}
		}
		#endregion

		Debug.Log(maxY.ToString("00") + minY.ToString("00") + "  " + maxX.ToString("00") + minX.ToString("00"));
		Debug.Log((maxY - minY).ToString ("00") + (maxX - minX).ToString ("00"));

		for(int y = minY; y <= maxY; y++)
		{
			for(int x = minX; x <= maxX; x++)
			{
				if(groundLayout[x,y] == null)
					groundLayer += 'Z';
				else
					groundLayer += groundLayout[x,y];

				if(entityLayout[x,y] == null)
					entityLayer += 'Z';
				else
					entityLayer += entityLayout[x,y];

				if(mechanismLayout[x,y] == null)
					mechanismLayer += 'Z';
				else
					mechanismLayer += mechanismLayout[x,y];

                if(mechanisms[x,y] != null)
                {
                    mechanismsText += mechanisms[x, y].group.ToString();
                    if (mechanisms[x, y].receivesInput)
                        mechanismsText += mechanisms[x, y].startOpen ? "1" : "0";
                    else
                        mechanismsText += "Z";
                }
			}
		}

		levelData.Add("id", "L0003");
		levelData.Add("name", levelName);
        levelData.Add("difficulty", levelDifficulty);
		levelData.Add("creator", "Michael");
		levelData.Add("colour", levelColor.r.ToString("000") + levelColor.g.ToString("000") + levelColor.b.ToString("000"));
		levelData.Add("dimensions", (maxX - minX + 1).ToString("00") + (maxY - minY + 1).ToString("00"));
        levelData.Add("groundlayer", Crypto.Compress(groundLayer));
        levelData.Add("entitylayer", Crypto.Compress(entityLayer));
        levelData.Add("mechanismlayer", Crypto.Compress(mechanismLayer));
        levelData.Add("mechanisms", mechanismsText);

        return levelData;
	}

    private IEnumerator SaveLevelWait()
    {
        Message("Saving...", false);
        yield return new WaitForSeconds(2);

        SaveLevel(levelAsJSON());
        LevelActionSelect("Back");
    }

    private void SaveLevel(Dictionary<string, object> levelData)
    {
        //TEMP ALPHA V0.1.0 PLEASE DELETE
        bool empty = true;
        for (int y = 0; y < currentLength; y++)
        {
            for (int x = 0; x < currentWidth; x++)
            {
                if (groundLayout[x, y] != null)
                    empty = false;
            }
        }
        if (empty)
        {
            SetNotification("Cannot save an empty level");
            return;
        }
        //----

        string serialized = Json.Serialize(levelData);
        System.IO.File.WriteAllText("User Levels\\" + levelName + ".lv", Crypto.Encrypt(serialized));
        if(levelSprite != questionMark)
            System.IO.File.WriteAllBytes("User Levels\\" + levelName + " Image.png", levelSprite.texture.EncodeToPNG());
        levelChanged = false;
        minorChange = false;
        SetNotification("Level \"" + levelName + "\"" + " saved successfully!");
    }

    public void TestLevel()
    {
        if (tooltipPanel.activeSelf)
        {
            if (helpEnabled)
            {
                helpEnabled = false;
                ShowRequirements(false);
                SelectionSwitched("GF#Floor");
                return;
            }
            else
                return;
        }

        //TEMP ALPHA V0.1.0 PLEASE DELETE
        bool finishExists = false;
        for (int y = 0; y < currentLength; y++)
        {
            for (int x = 0; x < currentWidth; x++)
            {
                if (groundLayout[x, y] == 'X')
                    finishExists = true;
            }
        }
        if (!finishExists)
            return;
        //----

        if (!System.IO.File.Exists("User Levels\\" + levelName + " Image.png") && levelSprite != questionMark)
            System.IO.File.WriteAllBytes("User Levels\\Temp Image.png", levelSprite.texture.EncodeToPNG());

        LevelLoader.levelToLoad = -1;
        LevelLoader.JSONToLoad = levelAsJSON();
        Application.LoadLevel(1);
    }

    private bool ValidMousePos()
    {
        Vector3 mousePos = Input.mousePosition;
        //TODO: Calculate dropped height correctly
        if (mousePos.x < 250 && (Screen.height - mousePos.y) < (125 + (DropDownList.droppedCount * 75)))
            return false;
        else
            return true;
    }

    public void SaveLevelSettings()
    {
        if (levelColor != GameObject.Find("Level Color Select").GetComponent<Image>().color || levelDifficulty != GameObject.Find("Difficulty Field").GetComponent<Text>().text || levelSprite != settingsPanel.transform.FindChild("Level Image Select").transform.FindChild("Level Image").GetComponent<Image>().sprite)
            minorChange = true;

        levelName = GameObject.Find("Level Name Input").GetComponent<InputField>().text;
        Camera.main.backgroundColor = GameObject.Find("Level Color Select").GetComponent<Image>().color;
        levelColor = GameObject.Find("Level Color Select").GetComponent<Image>().color;
        levelDifficulty = GameObject.Find("Difficulty Field").GetComponent<Text>().text;
        lastLevelSprite = levelSprite;

        LevelActionSelect("Back");
    }

    public void CancelLevelSettings()
    {
        GameObject.Find("Level Name Input").GetComponent<InputField>().text = levelName;
        GameObject.Find("Level Color Select").GetComponent<Image>().color = (Color)levelColor;
        RedInput.value = levelColor.r;
        GreenInput.value = levelColor.g;
        BlueInput.value = levelColor.b;
        GameObject.Find("Difficulty Field").GetComponent<Text>().text = levelDifficulty;
        levelSprite = lastLevelSprite;

        LevelActionSelect("Back");
    }

    public void LevelActionSelect(string action)
    {
        if(action == "Settings")
        {
            settingsPanel.SetActive(true);
            ClearTinker();
            activePanel = settingsPanel;
            GameObject.Find("Difficulty Field").GetComponent<Text>().text = levelDifficulty;
            GameObject.Find("Level Name Input").GetComponent<InputField>().text = levelName;
            GameObject.Find("Level Color Select").GetComponent<Image>().color = (Color)levelColor;
            RedInput.value = levelColor.r;
            GreenInput.value = levelColor.g;
            BlueInput.value = levelColor.b;
            Camera.main.GetComponent<CameraControl>().disableRotation = true;
            selectionPanel.SetActive(false);

            if (levelSprite == questionMark)
                settingsPanel.transform.FindChild("Level Image Select").transform.FindChild("Level Image").GetComponent<Image>().color = Color.black;
            else
                settingsPanel.transform.FindChild("Level Image Select").transform.FindChild("Level Image").GetComponent<Image>().color = Color.white;

            settingsPanel.transform.FindChild("Level Image Select").transform.FindChild("Level Image").GetComponent<Image>().sprite = levelSprite;
        }
        else if(action == "Exit")
        {
            if (levelChanged || minorChange)
            {
                exitPanel.SetActive(true);
                activePanel = exitPanel;
                Camera.main.GetComponent<CameraControl>().disableRotation = true;
                selectionPanel.SetActive(false);
            }
            else
                ExitEditor();

        }
        else if(action == "Save")
        {
            if(levelName == "")
                SetNotification("You need to set a level name in order to save the level");
            else
                StartCoroutine(SaveLevelWait());
        }
        else if(action == "Load")
        {
            openLevelPanel.SetActive(true);
            ClearTinker();
            activePanel = openLevelPanel;
            selectionPanel.SetActive(false);
            Camera.main.GetComponent<CameraControl>().disableRotation = true;
            PopulateLevelList();
        }
        else if(action == "Back")
        {
            tooltipPanel.SetActive(false);
            activePanel.SetActive(false);
            selectionPanel.SetActive(true);
            activePanel = selectionPanel;
            Camera.main.GetComponent<CameraControl>().disableRotation = false;
        }
        else if(action == "Upload")
        {
            if (tooltipPanel.activeSelf)
            {
                if (helpEnabled)
                {
                    helpEnabled = false;
                    ShowRequirements(false);
                    SelectionSwitched("GF#Floor");
                    return;
                }
                else
                    return;
            }

            //TEMP ALPHA V0.1.0 PLEASE DELETE
            bool finishExists = false;
            for (int y = 0; y < currentLength; y++)
            {
                for (int x = 0; x < currentWidth; x++)
                {
                    if (groundLayout[x, y] == 'X')
                        finishExists = true;
                }
            }
            if (!finishExists)
                return;
            //----

            selectionPanel.SetActive(false);
            uploadPanel.SetActive(true);
            activePanel = uploadPanel;
            Camera.main.GetComponent<CameraControl>().disableRotation = true;
        }
        else if(action == "Clear")
        {
            ClearLevel();
        }
        else if(action == "Start Screenshot")
        {
            settingsPanel.SetActive(false);
            screenshotPanel.SetActive(true);
            activePanel = screenshotPanel;
            Camera.main.GetComponent<CameraControl>().disableRotation = false;

            //Temporarily disable objects that would get in the way of the screenshot
            widthArrows.SetActive(false);
            lengthArrows.SetActive(false);
            Camera.main.GetComponent<Grid>().showGrid = false;
        }
        else if(action == "Capture Screenshot")
        {
            TakeScreenshot();
            screenshotPanel.SetActive(false);
            settingsPanel.SetActive(true);
            activePanel = settingsPanel;
            Camera.main.GetComponent<CameraControl>().disableRotation = true;
            settingsPanel.transform.FindChild("Level Image Select").transform.FindChild("Level Image").GetComponent<Image>().color = Color.white;
            settingsPanel.transform.FindChild("Level Image Select").transform.FindChild("Level Image").GetComponent<Image>().sprite = levelSprite;

            //Re-enable the objects that were disabled to take the screenshot
            widthArrows.SetActive(true);
            lengthArrows.SetActive(true);
            Camera.main.GetComponent<Grid>().showGrid = true;
        }
    }

    private void PopulateLevelList()
    {
        string[] levels = System.IO.Directory.GetFiles("User Levels");
        List<Dropdown.OptionData> formattedLevels = new List<Dropdown.OptionData>();

        formattedLevels.Add(new Dropdown.OptionData("-"));

        foreach (string level in levels)
        {
            string name = level.Split('\\')[1];
            if (!name.Contains(".png"))
            {
                formattedLevels.Add(new Dropdown.OptionData(name.Remove(name.Length - 3)));
            }
        }

        GameObject.Find("Open Level Dropdown").GetComponent<Dropdown>().options = formattedLevels;
    }

    //Allow the user to change the color of the level background
    public void ColourTextChanged(string id)
    {
        int colourVal = 0;

        if (id == "R")
            colourVal = (int)RedInput.value;
        else if (id == "G")
            colourVal = (int)GreenInput.value;
        else if (id == "B")
            colourVal = (int)BlueInput.value;

        RedInput.transform.parent.GetComponent<Image>().color = new Color(id == "R" ? colourVal / 255f : RedInput.transform.parent.GetComponent<Image>().color.r, id == "G" ? colourVal / 255f : RedInput.transform.parent.GetComponent<Image>().color.g, id == "B" ? colourVal / 255f : RedInput.transform.parent.GetComponent<Image>().color.b);
    }

    //Will get back to this when i have figured out how to implement sessions properly
    //IEnumerator UploadLevel(string JSONData)
	//{
	//	byte[] levelData = System.Text.Encoding.UTF8.GetBytes(JSONData);
	//	WWWForm form = new WWWForm();

	//	form.AddField("action", "upload");
	//	form.AddField("user", "Walshy");
	//	form.AddBinaryData("upfile", levelData, "L0001", "application/octet-stream");

	//	if(sessionID != "")
	//		form.AddField("SessionID", sessionID);

	//	WWW w = new WWW("127.0.0.1/ButtonsAndBoxes/uploadLevel.php", form);

	//	yield return w;
	//	if(w.error != null)
	//		Debug.Log(w.error);
	//	else
	//		Debug.Log(w.text);

	//	if(w.responseHeaders.ContainsKey("SET-COOKIE"))
	//	{
	//		Debug.Log(w.responseHeaders ["SET-COOKIE"]);
	//		try
	//		{
	//			sessionID = w.responseHeaders["SET-COOKIE"].Split(new char[2]{'=',';'})[1];
	//		}
	//		catch
	//		{
	//			Debug.Log("Unknown cookie");
	//			sessionID = "";
	//		}
	//	}
	//}

	IEnumerator GetUserFromSession()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("yar", "arg");
		if(sessionID != "")
			form.AddField("SID", sessionID);
		WWW w = new WWW("127.0.0.1/ButtonsAndBoxes/getUserSession.php", form);

		yield return w;

		Debug.Log(w.text);
	}

    IEnumerator UploadLevel(string JSONData, string creator)
    {
        WWWForm form = new WWWForm();
        byte[] levelData = System.Text.Encoding.UTF8.GetBytes(JSONData);
        char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        string tempName = "";
        System.Random random = new System.Random();

        //Randomize temp name
        for (int i = 0; i < 10; i++)
        {
            tempName += chars[random.Next(chars.Length)];
        }

        form.AddField("action", "upload");
        form.AddField("creator", creator);
        form.AddBinaryData("uploaded_file", levelData, tempName, "application/octet-stream");

        //WWW w = new WWW("127.0.0.1/uploadLevel.php", form);
        WWW w = new WWW("michael-walsh.co.uk/uploadLevel.php", form);

        yield return w;

        if (w.error != null)
        {
            Debug.Log(w.error);
            Message(w.error, true);
        }
        else
        {
            LevelActionSelect("Back");
            if (w.text == "Success")
                SetNotification("Level \"" + levelName + "\" uploaded successfully!");
            else
                SetNotification(w.text);
        }
    }

    public void UploadLevelButton()
    {
        if(uploadPanel.transform.FindChild("Creator Name Input").GetComponent<InputField>().text.Length < 2)
            return;

        Message("Uploading", false);
        StartCoroutine(UploadLevel(Json.Serialize(levelAsJSON()), uploadPanel.transform.FindChild("Creator Name Input").GetComponent<InputField>().text));
    }

    public void ExitEditor()
    {
        Application.LoadLevel(0);
    }

    public void SaveExit()
    {
        if (levelName == "")
        {
            LevelActionSelect("Back");
            SetNotification("You need to set a level name in order to save the level");
        }
        else
        {
            SaveLevel(levelAsJSON());
            ExitEditor();
        }
    }

    //Change the level details shown to the user based on what level they selected
    public void LevelSelectChange()
    {
        Texture2D newLevelImage = null;
        string levelPath = "User Levels\\" + GameObject.Find("Open Level Field").GetComponent<Text>().text;
        Dictionary<string, object> levelData = Json.Deserialize(Crypto.Decrypt(System.IO.File.ReadAllText(levelPath + ".lv"))) as Dictionary<string, object>;

        //Exit method if selected level file does not exist
        try
        {
            //Display level image to user if one exists
            if (System.IO.File.Exists(levelPath + " Image.png"))
            {
                newLevelImage = new Texture2D(720, 480);
                newLevelImage.LoadImage(System.IO.File.ReadAllBytes(levelPath + " Image.png"));
                levelInformation.FindChild("Open Level Image").GetComponent<Image>().color = Color.white;
                levelInformation.FindChild("Open Level Image").GetComponent<Image>().sprite = Sprite.Create(newLevelImage, new Rect(0, 0, newLevelImage.width, newLevelImage.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                levelInformation.FindChild("Open Level Image").GetComponent<Image>().color = Color.black;
                levelInformation.FindChild("Open Level Image").GetComponent<Image>().sprite = questionMark;
            }

            Debug.Log((string)levelData["name"]);
            levelInformation.FindChild("Open Level Name Text").GetComponent<Text>().text =  "Level Name: " + (string)levelData["name"];
            levelInformation.FindChild("Open Level Difficulty Text").GetComponent<Text>().text = "Difficulty: " + (string)levelData["difficulty"];
            levelInformation.FindChild("Open Level Dimensions Text").GetComponent<Text>().text = "Dimensions: " + ((string)levelData["dimensions"]).Substring(0,2) + " x " + ((string)levelData["dimensions"]).Substring(2, 2);
        }
        catch
        {
            Message("Error: Selected Level Not Found", true);
            return;
        }
        finally
        {
            levelData = new Dictionary<string, object>();
        }
    }

    private void Message(string message, bool error)
    {
        activePanel.SetActive(false);
        messagePanel.SetActive(true);

        messageText.SetActive(error? false : true);
        errorText.SetActive(error? true : false);

        if (error)
            errorText.GetComponent<Text>().text = message;
        else
            messageText.GetComponent<Text>().text = message;

        activePanel = messagePanel;
        messagePanel.transform.FindChild("Okay Button").gameObject.SetActive(error ? true : false);
    }

    //private void ExitMessage()
    //{
    //    activePanel = selectionPanel;
    //    messagePanel.SetActive(false);
    //}
    
    private void OpenTinker()
    {
        tinkerPanel.SetActive(true);
        tinkerPanel.transform.FindChild("Group Dropdown").GetComponent<Dropdown>().value = selectedMechanism.group;

        if(tinkerPanel.transform.FindChild("Chosen Mechanism").childCount > 0)
            Destroy(tinkerPanel.transform.FindChild("Chosen Mechanism").GetChild(0).gameObject);

        currentObject = Instantiate(GameData.MechanismTypes[selectedMechanism.ID], tinkerPanel.transform.FindChild("Chosen Mechanism").position, Quaternion.Euler(300,180,135)) as GameObject;
        currentObject.transform.parent = tinkerPanel.transform.FindChild("Chosen Mechanism");
        currentObject.transform.localScale = new Vector3(1,1,1);
        currentObject.GetComponent<Renderer>().material.mainTexture = Resources.Load("Textures\\" + GameData.MechanismTypes[selectedMechanism.ID].name + selectedMechanism.GetComponent<Mechanism>().group.ToString()) as Texture;

        tinkerPanel.transform.FindChild("Mechanism Name").GetComponent<Text>().text = GameData.MechanismTypes[selectedMechanism.ID].name;

        //TODO: Implement doors being able to start open
        //if (selectedMechanism.receivesInput)
        //{
        //    tinkerStartDropdown.SetActive(true);
        //    tinkerStartDropdown.GetComponent<Dropdown>().value = selectedMechanism.startOpen ? 1 : 0;
        //}
        //else
        //{
        //    tinkerStartDropdown.SetActive(false);
        //}
    }

    public void TinkerGroupChanged(int group)
    {
        levelChanged = true;
        selectedMechanism.group = (byte)(tinkerPanel.transform.FindChild("Group Dropdown").GetComponent<Dropdown>().value);
        selectedMechanism.GetComponent<Renderer>().material.mainTexture = Resources.Load("Textures\\" + GameData.MechanismTypes[selectedMechanism.ID].name + selectedMechanism.GetComponent<Mechanism>().group.ToString()) as Texture;
        tinkerPanel.transform.FindChild("Chosen Mechanism").GetChild(0).GetComponent<Renderer>().material.mainTexture = Resources.Load("Textures\\" + GameData.MechanismTypes[selectedMechanism.ID].name + selectedMechanism.GetComponent<Mechanism>().group.ToString()) as Texture;
    }

    public void TakeScreenshot()
    {
        RenderTexture rt = new RenderTexture(720,480,24);
        Camera.main.targetTexture = rt;
        Texture2D levelImage = new Texture2D(720, 480, TextureFormat.RGB24, false);

        //Take the screenshot
        Camera.main.Render();

        //Now save the screenshot to a variable and clean up
        RenderTexture.active = rt;
        levelImage.ReadPixels(new Rect(0, 0, 720, 480), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        levelImage.LoadImage(levelImage.EncodeToPNG());
        lastLevelSprite = levelSprite;
        levelSprite = Sprite.Create(levelImage, new Rect(0, 0, levelImage.width, levelImage.height), new Vector2(0.5f, 0.5f));
    }

    private void ShowRequirements(bool upload)
    {
        string requirements = "";
        int lineCount = 0;
        int playerCount = 0;
        bool finishExists = false;

        for(int y = 0; y < currentLength; y++)
        {
            for(int x = 0; x < currentWidth; x++)
            {
                if(groundLayout[x, y] == 'X')
                    finishExists = true;
                if(entityLayout[x, y] == 'P')
                    playerCount++;
            }
        }

        if (!finishExists)
        {
            requirements += "- There must be at least one finish block\n";
            lineCount += 2;
        }
        if (playerCount == 0)
        {
            requirements += "- There must be a player entity\n";
            lineCount += 2;
        }
        if(playerCount > 1)
        {
            requirements += "- There can only be one player entity\n";
            lineCount += 2;
        }
        if (upload)
        {
            if (levelName == "")
            {
                requirements += "- You must set a level name in settings\n";
                lineCount += 2;
            }
            if (!levelComplete)
            {
                requirements += "- You must complete the level yourself before uploading it\n";
                lineCount += 3;
            }
            if (levelSprite == questionMark)
            {
                requirements += "- You must set a level image in settings\n";
                lineCount += 2;
            }       
        }

        if (requirements == "")
            return;

        tooltipPanel.SetActive(true);
        tooltipPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 8 + (16 * lineCount));
        tooltipPanel.transform.GetChild(0).GetComponent<Text>().text = requirements;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    public void ShowTooltip(string id)
    {
        if (!helpEnabled)
        {
            if (id == "Play" || id == "Upload")
                ShowRequirements(id == "Play" ? false : true);
            return;
        }

        string helpString = "";
        int lineCount = 0;

        switch(id)
        {
            case "Settings":
                helpString = "Allows you to change the name, intended difficulty and color of the level.";
                lineCount = 3;
                break;
            case "Save":
                helpString = "Allows you to save the level, providing they have set a level name.";
                lineCount = 3;
                break;
            case "Play":
                helpString = "Play through the level you have built to test its elements.";
                lineCount = 3;
                break;
            case "Upload":
                helpString = "Upload your finished level so that other players can play through it.";
                lineCount = 4;
                break;
            case "Load":
                helpString = "Open previous finished or part-built levels and edit them.";
                lineCount = 3;
                break;
            case "Tinker":
                helpString = "Select mechanisms and change their more intricate settings.";
                lineCount = 3;
                break;
            case "Exit":
                helpString = "Exit the level editor and return to the main menu.";
                lineCount = 2;
                break;
            case "Help":
                helpString = "Selecting this and highlighting other buttons will show you what they do.";
                lineCount = 4;
                break;
            case "Clear":
                helpString = "Clear the current level elements.";
                lineCount = 2;
                break;
            case "Floor":
                helpString = "A standard floor block, players can move on this without any side effects.";
                lineCount = 3;
                break;
            case "Ice":
                helpString = "An ice block, when players move onto ice, they cannot stop until they are not on an ice block.";
                lineCount = 5;
                break;
            case "Pit":
                helpString = "A pit, pushing crates into this will create a platform that acts as a floor block.";
                lineCount = 3;
                break;
            case "Finish":
                helpString = "A finish block, when a player moves onto this, the level is complete.";
                lineCount = 3;
                break;
            case "Wall":
                helpString = "A wall block, acts as an immovable obstruction to any entities.";
                lineCount = 3;
                break;
            case "Crate":
                helpString = "A standard crate, can be moved when pushed by players or when another crate is pushed into it on ice.";
                lineCount = 5;
                break;
            case "Heavy Crate":
                helpString = "A heavy crate, at the moment, it behaves exactly the same as a normal crate.";
                lineCount = 4;
                break;
            case "Player":
                helpString = "The player object, only one of these can exist at a time.";
                lineCount = 3;
                break;
            case "Button":
                helpString = "A button, can be pushed down by an entity above it. When all buttons of one color are pushed down, the corresponding door(s) will be activated.";
                lineCount = 6;
                break;
            case "Door":
                helpString = "A door, will be activated when all buttons of the same color are pushed down.";
                lineCount = 4;
                break;
        }

        if(helpString != "")
        {
            tooltipPanel.SetActive(true);
            tooltipPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 8 + (16 * lineCount));
            tooltipPanel.transform.GetChild(0).GetComponent<Text>().text = helpString;
        }
    }

    //Called when the user wishes to amend a previously created level
    public void LoadLevelButton()
    {
        if (System.IO.File.Exists("User Levels\\" + GameObject.Find("Open Level Field").GetComponent<Text>().text + ".lv"))
        {
            StartCoroutine(LoadLevelWait("User Levels\\" + GameObject.Find("Open Level Field").GetComponent<Text>().text + ".lv"));
            Message("Loading", false);
        }
        else
        {
            Message("Can't find file for selected level.", true);
        }
    }

    private void SetNotification(string message)
    {
        notificationText.SetActive(true);
        notificationText.GetComponent<NotificationMessage>().SetMessage(message);
    }

    //Called when the user wants to load a previously saved level
    private void LoadLevel(string levelPath)
    {
        LoadLevel(Json.Deserialize(Crypto.Decrypt(System.IO.File.ReadAllText(levelPath))) as Dictionary<string, object>);
        LevelActionSelect("Back");
    }

    IEnumerator LoadLevelWait(string levelPath)
    {
        ClearLevel();
        yield return new WaitForSeconds(2);
        LoadLevel(levelPath);
        SetNotification("Loaded Level \"" + levelName + "\"");
    }

    //Called when returning from testing a level
    private void LoadLevel(Dictionary<string, object> levelData)
    {
        Level levelToLoad = LevelLoader.LoadFromJSON(levelData, -1, true);
        Texture2D loadedLevelImage = new Texture2D(720,480);
        levelColor = levelToLoad.BackgroundColor;

        Debug.Log(levelColor.r.ToString());
        Debug.Log(levelColor.g.ToString());
        Debug.Log(levelColor.b.ToString());

        levelName = levelToLoad.Name;
        levelDifficulty = levelToLoad.Difficulty;

        if (System.IO.File.Exists("User Levels\\" + levelName + " Image.png") || System.IO.File.Exists("User Levels\\Temp Image.png"))
        {
            loadedLevelImage.LoadImage(System.IO.File.Exists("User Levels\\" + levelName + " Image.png")? System.IO.File.ReadAllBytes("User Levels\\" + levelName + " Image.png") : System.IO.File.ReadAllBytes("User Levels\\Temp Image.png"));
            levelSprite = Sprite.Create(loadedLevelImage, new Rect(0, 0, loadedLevelImage.width, loadedLevelImage.height), new Vector2(0.5f, 0.5f));
            lastLevelSprite = levelSprite;
        }

        if (levelToLoad.Width < 4)
            currentWidth = 4;
        else
            currentWidth = levelToLoad.Width;

        if (levelToLoad.Length < 4)
            currentLength = 4;
        else
            currentLength = levelToLoad.Length;

        for (int y = 0; y < levelToLoad.Length; y ++)
        {
            for(int x = 0; x < levelToLoad.Width; x++)
            {
                if(levelToLoad.GroundLayout[x,y] != 'Z')
                    groundLayout[x, y] = levelToLoad.GroundLayout[x, y];
                if(levelToLoad.EntityLayout[x, y] != null)
                {
                    if (levelToLoad.EntityLayout[x, y].ID == 'P')
                    {
                        playerX = x;
                        playerY = y;
                    }
                    entityLayout[x, y] = levelToLoad.EntityLayout[x, y].ID;
                }
                if (levelToLoad.MechanismLayout[x, y] != null)
                {
                    mechanismLayout[x, y] = levelToLoad.MechanismLayout[x, y].ID;
                    mechanisms[x, y] = levelToLoad.MechanismLayout[x, y];
                }
                
				Camera.main.GetComponent<Grid>().SetGridSize((currentWidth * 2) - 1, (currentLength * 2) - 1);
				lengthArrows.transform.position = new Vector3(currentWidth - 3,0,(currentLength * 2) + 1);
				widthArrows.transform.position = new Vector3((currentWidth * 2) + 1,0,currentLength + 1);
				CheckArrows();
            }
        }
        Camera.main.GetComponent<CameraControl>().SetPivotPoint(new Vector3(currentWidth - 1, 0, currentLength - 1));
        levelChanged = false;
        levelComplete = false;
    }
}