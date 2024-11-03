using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;

namespace M183.Controllers.Helper
{
  public static class MD5Helper
  {
    public static string ComputeMD5Hash(string password, string salt)
    {
      using (var md5 = MD5.Create())
      {
        var saltedPassword = salt + password;
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
      }
    }

    public static string GenerateSalt()
    {
      byte[] saltBytes = new byte[16];
      using (var rng = new RNGCryptoServiceProvider())
      {
        rng.GetBytes(saltBytes);
      }
      return BitConverter.ToString(saltBytes).Replace("-", "").ToLower();
    }
  }
}
