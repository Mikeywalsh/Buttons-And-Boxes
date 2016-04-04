using UnityEngine;
using System;
using System.Collections.Generic;

public static class GameData {

	private static Dictionary<char, GameObject> _groundTypes = new Dictionary<char, GameObject>();
	private static Dictionary<char, GameObject> _entityTypes = new Dictionary<char, GameObject>();
	private static Dictionary<char, GameObject> _mechanismTypes = new Dictionary<char, GameObject>();

	public static bool initialized = false;

    public static void Initialize()
	{
        try
        {
            Dictionary<string, object> blockData = MiniJSON.Json.Deserialize((Resources.Load("BlockData") as TextAsset).text) as Dictionary<string, object>;
            List<object> groundData = blockData["groundtypes"] as List<object>;
            List<object> entityData = blockData["entitytypes"] as List<object>;
            List<object> mechanismData = blockData["mechanismtypes"] as List<object>;

            for (int i = 0; i < groundData.Count; i++)
            {
                Dictionary<string, object> currentBlock = groundData[i] as Dictionary<string, object>;
                _groundTypes.Add(((string)currentBlock["key"])[0], Resources.Load((string)currentBlock["name"]) as GameObject);
            }

            for (int i = 0; i < entityData.Count; i++)
            {
                Dictionary<string, object> currentBlock = entityData[i] as Dictionary<string, object>;
                _entityTypes.Add(((string)currentBlock["key"])[0], Resources.Load((string)currentBlock["name"]) as GameObject);
            }

            for (int i = 0; i < mechanismData.Count; i++)
            {
                Dictionary<string, object> currentBlock = mechanismData[i] as Dictionary<string, object>;
                _mechanismTypes.Add(((string)currentBlock["key"])[0], Resources.Load((string)currentBlock["name"]) as GameObject);
            }

            initialized = true;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            Application.Quit();
        }
    }

	public static Dictionary<char, GameObject> GroundTypes
	{
		get{ return _groundTypes; }
	}

	public static Dictionary<char, GameObject> EntityTypes
	{
		get{ return _entityTypes; }
	}

	public static Dictionary<char, GameObject> MechanismTypes
	{
		get{ return _mechanismTypes; }
	}
}