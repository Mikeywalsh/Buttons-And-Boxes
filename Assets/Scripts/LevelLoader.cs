using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

sealed public class LevelLoader : MonoBehaviour {

	public static int levelToLoad = 0;
    public static Dictionary<string, object> JSONToLoad;

    //Called when loading a level outside of the level editor
    public static Level Load(string path, int index)
    {
        string rawJSON = System.IO.File.ReadAllText(path);
        List<object> allLevels = Json.Deserialize(rawJSON) as List<object>;
        Dictionary<string, object> levelData = allLevels[index] as Dictionary<string, object>;

        return LoadFromJSON(levelData, index);
    }

	//Load Level from JSON
	public static Level LoadFromJSON(Dictionary<string, object> levelData, int index)
	{
		#region Throw Exceptions If JSON Data Does Not Contain All Neccessary Fields
		if(!levelData.ContainsKey("id"))
			throw new Exception("ID not present in level file!");
		if(!levelData.ContainsKey("name"))
			throw new Exception("Name not present in level file!");
        if (!levelData.ContainsKey("difficulty"))
            throw new Exception("Difficulty not present in level file!");
        if (!levelData.ContainsKey("creator"))
			throw new Exception("creator not present in level file!");
		if(!levelData.ContainsKey("colour"))
			throw new Exception("Colour not present in level file!");
		if(!levelData.ContainsKey("dimensions"))
			throw new Exception("Dimensions not present in level file!");
		if(!levelData.ContainsKey("groundlayer"))
			throw new Exception("Ground layout not present in level file!");
		if(!levelData.ContainsKey("entitylayer"))
			throw new Exception("Entity layout not present in level file!");
		if(!levelData.ContainsKey("mechanismlayer"))
			throw new Exception("Mechanism layout not present in level file!");
        #endregion

        string name = (string)levelData["name"];
        string difficulty = (string)levelData["difficulty"];
        int levelWidth = int.Parse(((string)levelData["dimensions"]).Substring(0,2));
		int levelHeight = int.Parse(((string)levelData["dimensions"]).Substring(2,2));
		Color backgroundColor = (Color)new Color32(byte.Parse(((string)levelData["colour"]).Substring(0,3)), byte.Parse(((string)levelData["colour"]).Substring(3,3)), byte.Parse(((string)levelData["colour"]).Substring(6,3)), 1);
		string rawGroundLayer = Crypto.Decompress((string)levelData["groundlayer"]);
		string rawEntityLayer = Crypto.Decompress((string)levelData["entitylayer"]);
		string rawMechanismLayer = Crypto.Decompress((string)levelData["mechanismlayer"]);

		char[,] groundLayer = new char[levelWidth, levelHeight];
		char[,] entityLayer = new char[levelWidth, levelHeight];
		char[,] mechanismLayer = new char[levelWidth, levelHeight];
		Entity[,] entities = new Entity[levelWidth, levelHeight];
		Mechanism[,] mechanisms = new Mechanism[levelWidth, levelHeight];
		int[] mechanismInputs = new int[2]{0,0};

		GameObject currentObject;
		int filePlayerCount = 0;

		#region More Exceptions For Level Data That Has Been Corrupted/Tampered With
		for(int x = 0; x < (levelWidth * levelHeight); x++)
		{
			if(rawEntityLayer[x] == 'P')
				filePlayerCount++;
		}
		if(!rawGroundLayer.Contains("X"))
		   throw new Exception("There Must Be Atleast One Finish Block");

		if(filePlayerCount != 1)
			throw new Exception("Invalid Player Object Count");

		if(((string)levelData["colour"]).Length != 9)
			throw new Exception("Invalid Background Colour");

		if(rawGroundLayer.Length != levelHeight * levelWidth || rawEntityLayer.Length != levelHeight * levelWidth || rawMechanismLayer.Length != levelHeight * levelWidth)
			throw new Exception("Invalid Block Count");
		#endregion

		Camera.main.GetComponent<CameraControl>().ChangeBackgroundColour(backgroundColor);

		for(int y = 0; y < levelHeight; y++)
		{
			for(int x = 0; x < levelWidth; x++)
			{
				groundLayer[x,y] = rawGroundLayer[x + (levelWidth * y)];
				if(groundLayer[x,y] != 'Z')
				{
					currentObject = Instantiate(GameData.GroundTypes[groundLayer[x,y]], new Vector3(x*2, index == -1? -1 : -2, y*2), Quaternion.identity) as GameObject;
					currentObject.transform.parent = GameObject.Find("Level Objects").transform;
                    currentObject.name = 'G' + x.ToString("00") + y.ToString("00");
                    currentObject.GetComponent<Block>().Spawn(x, groundLayer[x, y]);
				}

				entityLayer[x,y] = rawEntityLayer[x + (levelWidth * y)];
				if(entityLayer[x,y] != 'Z')
				{
					if(entityLayer[x,y] != 'P' || Application.loadedLevel == 3)
					{
						currentObject = Instantiate(GameData.EntityTypes[entityLayer[x,y]], new Vector3(x*2, (index == -1)? 1 : 0, y*2), Quaternion.identity) as GameObject;
						currentObject.transform.parent = GameObject.Find("Level Objects").transform;
                        currentObject.name = 'E' + x.ToString("00") + y.ToString("00");
                        currentObject.GetComponent<Block>().Spawn(x, entityLayer[x, y]);
					}
					else
					{
						currentObject = GameObject.Find("Player");
						currentObject.transform.position = new Vector3(x*2, (index == -1)? 1 : 0, y*2);
					}
					currentObject.GetComponent<Entity>().SetDimensions((byte)x, (byte)y);
					entities[x,y] = currentObject.GetComponent<Entity>();

					if(groundLayer[x,y] == 'P' || groundLayer[x,y] == 'Z')
						throw new Exception("Entities Cannot Spawn Over Nothing");

					if(groundLayer[x,y] == 'X')
						throw new Exception("Entities Cannot Spawn Over A Finish Block");
				}

				mechanismLayer[x,y] = rawMechanismLayer[x + (levelWidth * y)];
				if(mechanismLayer[x,y] != 'Z')
				{
					currentObject = Instantiate(GameData.MechanismTypes[mechanismLayer[x,y]], new Vector3(x*2, (index == -1)? 1 : 0, y*2), Quaternion.Euler(new Vector3(-90,0,0))) as GameObject;
					currentObject.transform.parent = GameObject.Find("Level Objects").transform;
                    currentObject.name = 'M' + x.ToString("00") + y.ToString("00");
                    currentObject.GetComponent<Block>().Spawn(x, mechanismLayer[x, y]);
					mechanisms[x,y] = currentObject.GetComponent<Mechanism>();

					for(int g = 0; g < 2; g++)
					{
						if(!mechanisms[x,y].receivesInput && mechanisms[x,y].group == g)
							mechanismInputs[g] += 1;
					}

					if(groundLayer[x,y] == 'P' || groundLayer[x,y] == 'Z')
						throw new Exception("Mechanisms Cannot Spawn Over Nothing");

					if(groundLayer[x,y] == 'X')
						throw new Exception("Mechanisms Cannot Spawn On A Finish Block");

					if(!mechanisms[x,y].receivesInput && entityLayer[x,y] != 'Z')
						throw new Exception("Input mechanisms cannot spawn on entities");

					if(mechanisms[x,y].receivesInput && entityLayer[x,y] != 'Z' && !entities[x,y].moveable)
						throw new Exception("Only moveable entities can spawn inside mechanisms that receive input");
				}
			}
		}

		foreach(Mechanism m in mechanisms)
		{
			if(m != null && m.receivesInput)
				m.inputRequired = mechanismInputs[m.group];
		}

        return new Level(index, name, difficulty, backgroundColor, groundLayer, entities, mechanisms);
	}

	//Load Level With Specific ID From LevelData File
	public static Level Load(string path, string ID)
	{
		int levelIndex = 0;
		string rawJSON = System.IO.File.ReadAllText (path);
		List<object> allLevels = Json.Deserialize(rawJSON) as List<object>;
		
		for(int i = 0; i < allLevels.Count; i++)
		{
			if((string)((Dictionary<string,object>)allLevels[i])["id"] == ID)
			{
				levelIndex = 0;
				break;
			}
		}
		
		return(Load(path, levelIndex));
	}
}