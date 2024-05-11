namespace UTFClassAPI;

using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// Provides methods for cryptographic operations.
/// </summary>
public class Crypto
{
    private byte[] key;
    private byte[] iv;

    /// <summary>
    /// Initializes a new instance of the Crypto class.
    /// </summary>
    public Crypto()
    {
        InitializeKeys();
    }

    private void InitializeKeys()
    {
        string keysFilePath = GetKeysFilePath();

        if (File.Exists(keysFilePath))
        {
            ReadKeysFromFile();
        }
        else
        {
            this.key = RandomKey(256);
            this.iv = RandomIV();
            SaveKeysToFile();
        }
    }

    /// <summary>
    /// Encrypts the specified plain text.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted text.</returns>
    public string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = this.key;
            aesAlg.IV = this.iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    /// <summary>
    /// Decrypts the specified cipher text.
    /// </summary>
    /// <param name="cipherText">The cipher text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    public string Decrypt(string cipherText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = this.key;
            aesAlg.IV = this.iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    private byte[] RandomKey(int keySize)
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] key = new byte[keySize / 8];
            rng.GetBytes(key);
            return key;
        }
    }

    private byte[] RandomIV()
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] iv = new byte[16];
            rng.GetBytes(iv);
            return iv;
        }
    }

    private string GetKeysFilePath()
    {
        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Keys");
        Directory.CreateDirectory(folderPath);
        return Path.Combine(folderPath, "keys.dat");
    }

    private void SaveKeysToFile()
    {
        string filePath = GetKeysFilePath();
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            fs.Write(this.key, 0, this.key.Length);
            fs.Write(this.iv, 0, this.iv.Length);
        }
    }

    private void ReadKeysFromFile()
    {
        string filePath = GetKeysFilePath();
        if (File.Exists(filePath))
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                long keySize = fs.Length / 2;
                long ivSize = fs.Length - keySize;

                this.key = new byte[keySize];
                this.iv = new byte[16];

                fs.Read(this.key, 0, (int)keySize);
                fs.Read(this.iv, 0, 16);
            }
        }
    }

    /// <summary>
    /// Gets the secret key as a Base64 encoded string.
    /// </summary>
    /// <returns>The secret key.</returns>
    public string GetSecretKeyAsString()
    {
        return Convert.ToBase64String(this.key);
    }
}
