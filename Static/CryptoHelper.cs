using System;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"); // 32 bytes para AES-256 (guardar seguro)
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("ABCDEF0123456789"); // 16 bytes (guardar seguro)

    public static string EncryptToBase64(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }
    }

    public static string DecryptFromBase64(string base64Cipher)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherBytes = Convert.FromBase64String(base64Cipher);
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
