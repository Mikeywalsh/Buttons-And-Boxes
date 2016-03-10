using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using MiniJSON;

public class TempLevelUpdater : MonoBehaviour {
    //Temporary script used to compress and encrypt levels for the main game when they're finished
	void Start () {
        List<object> allLevels = Json.Deserialize(File.ReadAllText("LevelData")) as List<object>;

        for (int i = 0; i < allLevels.Count; i++)
        {
            ((Dictionary<string, object>)allLevels[i])["groundLayer"] = Crypto.Compress((string)((Dictionary<string, object>)allLevels[i])["groundLayer"]);
            ((Dictionary<string, object>)allLevels[i])["entityLayer"] = Crypto.Compress((string)((Dictionary<string, object>)allLevels[i])["entityLayer"]);
            ((Dictionary<string, object>)allLevels[i])["mechanismLayer"] = Crypto.Compress((string)((Dictionary<string, object>)allLevels[i])["mechanismLayer"]);
        }

        File.WriteAllText("Level_Data", Crypto.Encrypt(Json.Serialize(allLevels)));

    }
}
