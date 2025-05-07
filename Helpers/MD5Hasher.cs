using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace BachHoaXanh.Helpers
{

    public static class MD5Hasher
    {
        public static string HashPassword(string password)
        {
            using (var md5 = MD5.Create()) 
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                {
                    sb.Append(b.ToString("x2")); 
                }

                return sb.ToString();
            }
        }
    }
}
