using System.ComponentModel.DataAnnotations;

namespace M183.Models
{
  public class User
  {
    [Key]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public bool IsAdmin { get; set; }
    public string? SecretKey2FA { get; set; }
    public int? FailedLogins { get; set; }
  }
}
