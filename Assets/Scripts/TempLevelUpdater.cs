using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using MiniJSON;

//Series of manually called functions which change level files to newer versions and human readable formats
public class TempLevelUpdater : MonoBehaviour {
    
    void Start()
    {
        //AddMechanismData();
        //ShowRaw();
        //EncryptRaw();
        //ReplaceOldMechanisms();
        //CompressRaw();
    }

	void AddMechanismData()
    {
        List<object> allLevels = Json.Deserialize(File.ReadAllText("LevelDataRAW")) as List<object>;
        //List<object> allLevels = Json.Deserialize(Crypto.Decrypt(File.ReadAllText("LevelData"))) as List<object>;
        string mechanismData;
        string decompressedMechanismLayout;
        
        for (int i = 0; i < allLevels.Count; i++)
        {
            mechanismData = "";
            decompressedMechanismLayout = Crypto.Decompress((string)((Dictionary<string, object>)allLevels[i])["mechanismlayer"]);

            for (int j = 0; j < decompressedMechanismLayout.Length; j++)
            {
                if (decompressedMechanismLayout[j] != 'Z')
                {
                    if (decompressedMechanismLayout[j] == 'A' || decompressedMechanismLayout[j] == 'B')
                        mechanismData += '0';
                    else
                        mechanismData += '1';

                    if (decompressedMechanismLayout[j] == 'B' || decompressedMechanismLayout[j] == 'D')
                        mechanismData += '0';
                    else
                        mechanismData += 'Z';
                }
            }
            //((Dictionary<string, object>)allLevels[i])["mechanisms"] = mechanismData;
            ((Dictionary<string, object>)allLevels[i]).Add("mechanisms", mechanismData);
        }
        
        //File.WriteAllText("LevelData", Crypto.Encrypt(Json.Serialize(allLevels)));
        File.WriteAllText("LevelData", Json.Serialize(allLevels));
    }

    void CompressRaw()
    {
        //List<object> allLevels = Json.Deserialize(Crypto.Decrypt(File.ReadAllText("LevelData"))) as List<object>;
        List<object> allLevels = Json.Deserialize(File.ReadAllText("LevelData")) as List<object>;

        for (int i = 0; i < allLevels.Count; i++)
        {
            ((Dictionary<string, object>)allLevels[i])["groundlayer"] = Crypto.Compress((string)((Dictionary<string, object>)allLevels[i])["groundlayer"]);
            ((Dictionary<string, object>)allLevels[i])["entitylayer"] = Crypto.Compress((string)((Dictionary<string, object>)allLevels[i])["entitylayer"]);
            ((Dictionary<string, object>)allLevels[i])["mechanismlayer"] = Crypto.Compress((string)((Dictionary<string, object>)allLevels[i])["mechanismlayer"]);
        }

        //File.WriteAllText("LevelDataRAW", Json.Serialize(allLevels));
        File.WriteAllText("LevelData", Json.Serialize(allLevels));
    }

    void ShowRaw()
    {
        List<object> allLevels = Json.Deserialize(Crypto.Decrypt(File.ReadAllText("LevelData"))) as List<object>;
        File.WriteAllText("LevelDataRAW", Json.Serialize(allLevels));
    }

    void EncryptRaw()
    {
        List<object> allLevels = Json.Deserialize(File.ReadAllText("LevelDataRAW")) as List<object>;
        File.WriteAllText("LevelData", Crypto.Encrypt(Json.Serialize(allLevels)));
    }

    void ReplaceOldMechanisms()
    {
        List<object> allLevels = Json.Deserialize(Crypto.Decrypt(File.ReadAllText("LevelData"))) as List<object>;
        string decompressedMechanismLayout = "";
        string newLayout = "";

        for (int i = 0; i < allLevels.Count; i++)
        {
            decompressedMechanismLayout = Crypto.Decompress((string)((Dictionary<string, object>)allLevels[i])["mechanismlayer"]);

            for (int j = 0; j < decompressedMechanismLayout.Length; j++)
            {
                if (decompressedMechanismLayout[j] == 'C')
                    newLayout += 'A';
                else if (decompressedMechanismLayout[j] == 'D')
                    newLayout += 'B';
                else
                    newLayout += decompressedMechanismLayout[j];
            }
            ((Dictionary<string, object>)allLevels[i])["mechanismlayer"] = Crypto.Compress(newLayout);
        }

        File.WriteAllText("LevelDataRAW", Json.Serialize(allLevels));
    }
}