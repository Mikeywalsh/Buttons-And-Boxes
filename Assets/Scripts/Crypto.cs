﻿using System;
using System.IO;
using System.Security.Cryptography;

//Worst security ever, will make secure when rest of the project is fully playable
public class Crypto
{
    //Standard XOR with randomly generated 10-byte long key. Not secure at all!
    public static string Encrypt(string toEncrypt)
    {
        char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        char[] key = new char[10];
        string finalString = "";
        Random random = new Random();

        //Randomize key
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = chars[random.Next(chars.Length)];
        }

        //Add key to start on final string
        for (int i = 0; i < key.Length; i++)
        {
            finalString += (char)~key[i];
        }

        //Add XOR'd data to final string
        for (int i = 0; i < toEncrypt.Length; i++)
        {
            finalString += (char)(toEncrypt[i] ^ key[i % 10]);
        }
        
        return finalString; 
    }

    public static string Decrypt(string toDecrypt)
    {
        string key = toDecrypt.Substring(0, 10);
        string actualKey = "";
        string data = toDecrypt.Substring(10);
        string decryptedData = "";

        for (int i = 0; i < key.Length; i++)
        {
            actualKey += (char)~key[i];
        }

        for (int i = 0; i < data.Length; i++)
        {
            decryptedData += (char)(data[i] ^ actualKey[i % 10]);
        }

        return decryptedData;
    }

    //public static string Compress(string toCompress)
    //{
    //    string compressedData = "";
    //    char currentChar = toCompress[0];
    //    int occurence = 1;

    //    for(int i = 1; i < toCompress.Length; i++)
    //    {
    //        if (currentChar == toCompress[i] && i != toCompress.Length - 1)
    //            occurence++;
    //        else
    //        {
    //            compressedData += (occurence == 1? "": (occurence == 2? currentChar.ToString(): occurence.ToString())) + currentChar;
    //            currentChar = toCompress[i];
    //            occurence = 1;
    //        }
    //    }

    //    return compressedData;
    //}

    //public static string Decompress(string toDecompress)
    //{
    //    string decompressedData = "";
    //    int occurence;
    //    int offset = 0;

    //    for(int i = 0; i < toDecompress.Length; i ++)
    //    {
    //        if (int.TryParse(toDecompress[i].ToString(), out occurence))
    //        {
    //            for (int j = 0; j < occurence; j++)
    //                decompressedData += toDecompress[i];
    //        }
    //        else
    //            decompressedData += toDecompress[i];

    //    }

    //    return decompressedData;
    //}
}