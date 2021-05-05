using System;

namespace FinanceAPICore.Utilities
{
	public class SecurityService
	{
        public static string EncryptTripleDES(string Plaintext)
        {

            System.Security.Cryptography.TripleDESCryptoServiceProvider DES =

            new System.Security.Cryptography.TripleDESCryptoServiceProvider();

            System.Security.Cryptography.MD5CryptoServiceProvider hashMD5 =

            new System.Security.Cryptography.MD5CryptoServiceProvider();

            DES.Key = hashMD5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(_key));

            DES.Mode = System.Security.Cryptography.CipherMode.ECB;

            System.Security.Cryptography.ICryptoTransform DESEncrypt = DES.CreateEncryptor();

            var Buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(Plaintext);
            string TripleDES = Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));

            return TripleDES;

        }
        //Decryption Method 

        public static string DecryptTripleDES(string base64Text)
        {

            System.Security.Cryptography.TripleDESCryptoServiceProvider DES =

            new System.Security.Cryptography.TripleDESCryptoServiceProvider();

            System.Security.Cryptography.MD5CryptoServiceProvider hashMD5 =

            new System.Security.Cryptography.MD5CryptoServiceProvider();
            DES.Key = hashMD5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(_key));
            DES.Mode = System.Security.Cryptography.CipherMode.ECB;
            System.Security.Cryptography.ICryptoTransform DESDecrypt = DES.CreateDecryptor();
            var Buffer = Convert.FromBase64String(base64Text);

            string DecTripleDES = System.Text.ASCIIEncoding.ASCII.GetString(DESDecrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
            return DecTripleDES;

        }

        private static string _key = "At1a3f1n9nc38&n%g!r";
    }
}
