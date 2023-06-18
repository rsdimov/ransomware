using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        string[] folderPaths = new string[]
        {
            @"C:\Path\To\Folder1"
        };
        
        string password = "YourEncryptionPassword";

        foreach (string folderPath in folderPaths)
        {
            EncryptFolderContents(folderPath, password);
        }

        Console.WriteLine("Encryption completed.");
        Console.ReadLine();
    }

    static void EncryptFolderContents(string folderPath, string password)
    {
        string[] files = Directory.GetFiles(folderPath);

        foreach (string file in files)
        {
            EncryptFile(file, password);
        }

        string[] directories = Directory.GetDirectories(folderPath);

        foreach (string directory in directories)
        {
            EncryptFolderContents(directory, password);
        }
    }

    static void EncryptFile(string filePath, string password)
    {
        byte[] salt = GenerateRandomBytes(16);
        byte[] iv = GenerateRandomBytes(16);

        using (Aes aes = Aes.Create())
        {
            aes.Key = GenerateKey(password, salt);
            aes.IV = iv;

            using (FileStream inputFile = new FileStream(filePath, FileMode.Open))
            {
                using (FileStream outputFile = new FileStream(filePath + ".encrypted", FileMode.Create))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(outputFile, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Write the salt and IV to the output file
                        outputFile.Write(salt, 0, salt.Length);
                        outputFile.Write(iv, 0, iv.Length);

                        // Encrypt the file content
                        inputFile.CopyTo(cryptoStream);
                    }
                }
            }
        }

        File.Delete(filePath);
    }

    static byte[] GenerateRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
        }

        return randomBytes;
    }

    static byte[] GenerateKey(string password, byte[] salt)
    {
        using (Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(password, salt, 10000))
        {
            return keyGenerator.GetBytes(32); // 256-bit key
        }
    }
}
