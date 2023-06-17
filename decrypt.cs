using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        string folderPath = @"C:\Tools";
        string password = "YourEncryptionPassword";

        DecryptFolderContents(folderPath, password);

        Console.WriteLine("Decryption completed.");
        Console.ReadLine();
    }

    static void DecryptFolderContents(string folderPath, string password)
    {
        string[] files = Directory.GetFiles(folderPath);

        foreach (string file in files)
        {
            DecryptFile(file, password);
        }

        string[] directories = Directory.GetDirectories(folderPath);

        foreach (string directory in directories)
        {
            DecryptFolderContents(directory, password);
        }
    }

    static void DecryptFile(string filePath, string password)
    {
        if (!filePath.EndsWith(".encrypted"))
        {
            return;
        }

        string decryptedFilePath = filePath.Substring(0, filePath.Length - 10); // Remove ".encrypted" extension

        byte[] salt = new byte[16];
        byte[] iv = new byte[16];

        using (FileStream encryptedFile = new FileStream(filePath, FileMode.Open))
        {
            // Read the salt and IV from the encrypted file
            encryptedFile.Read(salt, 0, salt.Length);
            encryptedFile.Read(iv, 0, iv.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey(password, salt);
                aes.IV = iv;

                using (CryptoStream cryptoStream = new CryptoStream(encryptedFile, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (FileStream decryptedFile = new FileStream(decryptedFilePath, FileMode.Create))
                    {
                        // Decrypt the file content
                        cryptoStream.CopyTo(decryptedFile);
                    }
                }
            }
        }

        File.Delete(filePath);
    }

    static byte[] GenerateKey(string password, byte[] salt)
    {
        using (Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(password, salt, 10000))
        {
            return keyGenerator.GetBytes(32); // 256-bit key
        }
    }
}
