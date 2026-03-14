using System.Security.Cryptography;
using System.Text;

namespace Application.Helpers
{
    public class HashPassSHA256
    {
        public static string HashPass(string input)
        {
            using SHA256 HashAlgorithm = SHA256.Create();

            byte[] data = HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}