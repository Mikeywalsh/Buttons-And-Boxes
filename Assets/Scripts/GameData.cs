﻿using UnityEngine;
using System;
using System.Collections.Generic;

public static class GameData {

	private static Dictionary<char, GameObject> _groundTypes = new Dictionary<char, GameObject>();
	private static Dictionary<char, GameObject> _entityTypes = new Dictionary<char, GameObject>();
	private static Dictionary<char, GameObject> _mechanismTypes = new Dictionary<char, GameObject>();

	public static bool initialized = false;

    //public static string[] levelData = new string[]{"0806",
    //    "200180170",
    //    "FFFFFFFFFFFFPPFFFFFFPPFFFFFFPPXFFFFFPPFFFFFFFFFF",
    //    "WWWWWWWWWZZZZZZWWZZCZZZWWPZCZZZWWZZZZZZWWWWWWWWW",
    //    "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ",
    //    "1305",
    //    "180255180",
    //    "FFFFFFFFFFFZZFFFFFFFFFFFFFFFFFFFFFFFFXFFFFFFFFFFFFFFFFFFFFFFFFFZZ",
    //    "WWWWWWWWWWWZZWZZZZWZZZZWWWWPZCZCZZCZZZWWZZZZWZZZZWWWWWWWWWWWWWWZZ",
    //    "ZZZZZZZZZZZZZZZZZAZZZZCZZZZZZZZBZZZZDZZZZZZZZZZZCZZZZZZZZZZZZZZZZ",
    //    "0809",
    //    "155213155",
    //    "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFZFXFFFFFZFFFZZZZ",
    //    "WWWWWWWWWZZZWZZWWZWZWCZWWZZZZCZWWZWZWCZWWZZZWZZWWWZWWPZWZWZWWWWWZWWWZZZZ",
    //    "ZZZZZZZZZZZZZZZZZZZZZZAZZZZZZZAZZZZZZZAZZZZZZZZZZZBZZZZZZZZZZZZZZZZZZZZZ",
    //    "1209",
    //    "210210210",
    //    "FFFFFFFFFFFFFXPIIIIIIIIFFFFFFIIFIFIFZZZFIIIFIIIFZFFFIIIIIIFFZFFFIIIIIFIFZFFFIIIIIIIFZZZFIIFFIIIFZZZFFFFFFFFF",
    //    "WWWWWWWWWWWWWZZZZZZZZZZWWWWWWZZWZWZWZZZWZZZWZZZWZWWWZZZZCZWWZWPZZCZZZZZWZWWWZZZZZZZWZZZWZZWWZZZWZZZWWWWWWWWW",
    //    "ZZZZZZZZZZZZZZZBZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZAZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ",
    //    "1514",
    //    "255180180",
    //    "ZZZZFFFFFFFZZZZZZZZFFFFFFFZZZZZFFFFFFFFFFFFFZZIIIIIIIIIIIIIZFFIIFIIIIIFIIFFFFIIIIIIIIIIIFFFFIIIIFIFIIIIFFZFIIIIIIIIIIIFZZFIFIIIIIIIFIFZZFIIIIIFIIIIIFZZFFFFFFFFFFFFFZZZZFFFFFFFFFZZZZZZFFFFXFFFFZZZZZZZZZFFFZZZZZZ",
    //    "ZZZZWWWWWWWZZZZZZZZWZZPZZWZZZZZWWWWZCZCZWWWWZZWZZZZZZZZZZZWZWWZZWZZZZZWZZWWWZZZZZZZZZZZZZWWWZZZZWZWZZZZWWZWZZZZZZZZZZZWZZWZWZZZZZZZWZWZZWZZZZZWZZZZZWZZWWWZWWWWWZWWWZZZZWZZZZZZZWZZZZZZWWWWZWWWWZZZZZZZZZWWWZZZZZZ",
    //    "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZAZZZZZZZZZZZAZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZBZZZZZBZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ",
    //    "1009",
    //    "255180255",
    //    "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFZFFFFFFFFFZFFFFFFFFFZFXFFFFFFFZFFFFFFFFFZ",
    //    "WWWWWWWWWWWZZZZZZZZWWZZCZZWZZWWZZWCWWZWWWZCZZZZZWZWWWWWZWZWZWWWZCZZZWZWZZZZZPZWZWWWWWWWWWZ",
    //    "ZZZZZZZZZZZAAZZZZZZZZAAZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZBZZZZZZZZZZZZZZZZZ",
    //    "0512",
    //    "200200200",
    //    "ZZFFFZZFFFFFFIFFXFIFFFFIFFFFIFFFFIFFFFIFFFFIFFFFFFFFFFFFFFFF",
    //    "ZZWWWZZWZWWWWZWWZWZWWZWZWWZZZWWZCZWWPZZWWZZZWWZCZWWZZZWWWWWW",
    //    "ZZZZZZZZCZZZZZZZZZBZZDZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZAZZZZZZ",
    //    "1009",
    //    "180255255",
    //    "FFFFZZZZZZFPFFFFFFZZFFFFFFFFZZFFFFFFFFFFFFFFFFFFXFFFFFFFFFFFFFFFFFFFZZFPFFFFFFZZFFFFZZZZZZ",
    //    "WWWWZZZZZZWZZWWWWWZZWZZZZZZWZZWZZCWZZWWWWPZZCZZZZWWZZCWZZWWWWZZZZZZWZZWZZWWWWWZZWWWWZZZZZZ",
    //    "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZAZZZZZZZZZZCZDZZBZZZAZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ",
    //    "1212",
    //    "180180255",
    //    "ZZZZFFFFFZZZZZFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFZZFFFFFFFFFFZZFFFFFFFFFFZZZZZFFFFFZZZZZZZZFXFZZZZZZZZZFFFZZZZ",
    //    "ZZZZWWWWWZZZZZWWWZZZWWWWWWWZZZWZZZZWWZCZWZZZCWZWWZWCZZZWZCZWWZZZZWZZZZWWWWZWZCZCWZWZZWZZZZWZZZWZZWWWWZPZWWWZZZZZWWZWWZZZZZZZZWZWZZZZZZZZZWWWZZZZ",
    //    "ZZZZZZZZZZZZZZZZZAZAZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZAZAZZZZZZZZZZZZZZZZZZZZZZZZZZZAZZZZZZAZZZZZZZZZZZZZZZZZZZZBZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ"};

    public static void Initialize()
	{
		//<-- For Webplayer -->
		//_groundTypes.Add('F',Resources.Load("Floor Block") as GameObject);
		//_groundTypes.Add('P',Resources.Load("Pit") as GameObject);
		//_groundTypes.Add('I',Resources.Load("Ice Block") as GameObject);
		//_groundTypes.Add('X',Resources.Load("Finish Block") as GameObject);

		//_entityTypes.Add('C',Resources.Load("Crate") as GameObject);
		//_entityTypes.Add('H',Resources.Load("Heavy Crate") as GameObject);
		//_entityTypes.Add('P',Resources.Load("Player") as GameObject);
		//_entityTypes.Add('W',Resources.Load("Wall Block") as GameObject);

		//_mechanismTypes.Add('A',Resources.Load("Red Button") as GameObject);
		//_mechanismTypes.Add('B',Resources.Load("Red Door") as GameObject);
		//_mechanismTypes.Add('C',Resources.Load("Blue Button") as GameObject);
		//_mechanismTypes.Add('D',Resources.Load("Blue Door") as GameObject);

		//initialized = true;

        try
        {
            string[] fileContents;
            fileContents = System.IO.File.ReadAllLines("GroundTypes");

            for (int x = 0; x < fileContents.Length; x++)
            {
                if (fileContents[x].Length > 2)
                    _groundTypes.Add(fileContents[x][0], Resources.Load(fileContents[x].Substring(2)) as GameObject);
            }

            fileContents = System.IO.File.ReadAllLines("EntityTypes");

            for (int x = 0; x < fileContents.Length; x++)
            {
                if (fileContents[x].Length > 2)
                    _entityTypes.Add(fileContents[x][0], Resources.Load(fileContents[x].Substring(2)) as GameObject);
            }

            fileContents = System.IO.File.ReadAllLines("MechanismTypes");

            for (int x = 0; x < fileContents.Length; x++)
            {
                if (fileContents[x].Length > 2)
                    _mechanismTypes.Add(fileContents[x][0], Resources.Load(fileContents[x].Substring(2)) as GameObject);
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