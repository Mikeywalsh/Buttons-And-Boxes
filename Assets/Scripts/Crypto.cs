using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;

public class Crypto
{	
	public void Main(string[] Fileargs)
	{
		string dataFile;
		string signedFile;
		//If no file names are specified, create them. 
		if (Fileargs.Length < 2)
		{
			dataFile = @"text.txt";
			signedFile = "signedFile.enc";
			
			if (!File.Exists(dataFile))
			{
				// Create a file to write to. 
				using (StreamWriter sw = File.CreateText(dataFile))
				{
					sw.WriteLine("Here is a message to sign");
				}
			}
			
		}
		else
		{
			dataFile = Fileargs[0];
			signedFile = Fileargs[1];
		}
		try
		{
			// Create a random key using a random number generator. This would be the 
			//  secret key shared by sender and receiver. 
			byte[] secretkey = new Byte[64];
			//RNGCryptoServiceProvider is an implementation of a random number generator. 
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
				// The array is now filled with cryptographically strong random bytes.
			rng.GetBytes(secretkey);
				
				// Use the secret key to sign the message file.
			SignFile(secretkey, dataFile, signedFile);
				
				// Verify the signed file
			VerifyFile(secretkey, signedFile);
		}
		catch (IOException e)
		{
			Console.WriteLine("Error: File not found", e);
		}
		
	}

	public static void SignFile(byte[] key, String sourceFile, String destFile)
	{
		using (HMACSHA256 hmac = new HMACSHA256(key))
		{
			using (FileStream inStream = new FileStream(sourceFile, FileMode.Open))
			{
				using (FileStream outStream = new FileStream(destFile, FileMode.Create))
				{
					// Compute the hash of the input file. 
					byte[] hashValue = hmac.ComputeHash(inStream);
					// Reset inStream to the beginning of the file.
					inStream.Position = 0;
					// Write the computed hash value to the output file.
					outStream.Write(hashValue, 0, hashValue.Length);
					// Copy the contents of the sourceFile to the destFile. 
					int bytesRead;
					// read 1K at a time 
					byte[] buffer = new byte[1024];
					do
					{
						// Read from the wrapping CryptoStream.
						bytesRead = inStream.Read(buffer, 0, 1024);
						outStream.Write(buffer, 0, bytesRead);
					} while (bytesRead > 0);
				}
			}
		}
		return;
	}

	public static bool VerifyFile(byte[] key, String sourceFile)
	{
		bool valid = true;
		// Initialize the keyed hash object.  
		using (HMACSHA256 hmac = new HMACSHA256(key))
		{
			// Create an array to hold the keyed hash value read from the file. 
			byte[] storedHash = new byte[hmac.HashSize / 8];
			// Create a FileStream for the source file. 
			using (FileStream inStream = new FileStream(sourceFile, FileMode.Open))
			{
				// Read in the storedHash.
				inStream.Read(storedHash, 0, storedHash.Length);
				// Compute the hash of the remaining contents of the file. 
				// The stream is properly positioned at the beginning of the content,  
				// immediately after the stored hash value. 
				byte[] computedHash = hmac.ComputeHash(inStream);
				// compare the computed hash with the stored value 
				
				for (int i = 0; i < storedHash.Length; i++)
				{
					if (computedHash[i] != storedHash[i])
						valid = false;
				}
			}
		}
		return valid;		
	}	
}