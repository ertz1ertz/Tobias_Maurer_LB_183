using M183.Controllers.Dto;
using M183.Controllers.Helper;
using M183.Data;
using M183.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Authenticator;
using OtpSharp;
using QRCoder;

namespace M183.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LoginController : ControllerBase
  {
    private readonly NewsAppContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public LoginController(NewsAppContext context, IConfiguration configuration, ILogger<LoginController> logger)
    {
      _context = context;
      _configuration = configuration;
      _logger = logger;
    }

    /// <summary>
    /// Login a user using password and username
    /// </summary>
    /// <response code="200">Login successfull</response>
    /// <response code="400">Bad request</response>
    /// <response code="401">Login failed</response>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public ActionResult<User> Login(LoginDto request)
    {
      if (request == null || request.Username.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
      {
        _logger.LogInformation($"[{DateTime.Now}] user no user: Login failed: username or password empty");
        return BadRequest();
      }

      string username = request.Username;
      User? user = _context.Users
        .Where(u => u.Username == username)
        .FirstOrDefault();

      if (user == null)
      {
        _logger.LogInformation($"[{DateTime.Now}] user no user: Login failed: user doesn't exist");
        return Unauthorized("login failed");
      }

      string passwordHash = MD5Helper.ComputeMD5Hash(request.Password, user.Salt);

      if (user.Password != passwordHash)
      {
        user.FailedLogins += 1;

        if (user.FailedLogins >= 10)
        {
          _logger.LogCritical($"[{DateTime.Now}] user {user.Username}: Login failed: Login failed ten times in a row");
        }
        else
        {
          _logger.LogWarning($"[{DateTime.Now}] user {user.Username}: Login failed: password mismatch");
        }
        _context.Update(user);
        _context.SaveChanges();
        return Unauthorized("login failed");
      }
      if (user.SecretKey2FA != null)
      {
        string secretKey = user.SecretKey2FA;
        string userUniqueKey = user.Username + secretKey;
        TwoFactorAuthenticator authenticator = new TwoFactorAuthenticator();
        bool isAuthenticated = authenticator.ValidateTwoFactorPIN(userUniqueKey, request.UserKey);
        if (!isAuthenticated)
        {
          user.FailedLogins += 1;

          if (user.FailedLogins >= 10)
          {
            _logger.LogCritical($"[{DateTime.Now}] user {user.Username}: Login failed: Login failed ten times in a row");
          }
          else
          {
            _logger.LogWarning($"[{DateTime.Now}] user {user.Username}: Login failed: false 2FA code");
          }
          _context.Update(user);
          _context.SaveChanges();
          return Unauthorized("login failed");
        }
      }

      user.FailedLogins = 0;
      _context.Update(user);
      _context.SaveChanges();
      _logger.LogWarning($"[{DateTime.Now}] user {user.Username}: Login successgul: successful login");
      return Ok(CreateToken(user));
    }

    private string CreateToken(User user)
    {
      string issuer = _configuration.GetSection("Jwt:Issuer").Value!;
      string audience = _configuration.GetSection("Jwt:Audience").Value!;

      List<Claim> claims = new List<Claim> {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim(ClaimTypes.Role,  (user.IsAdmin ? "admin" : "user"))
            };

      string base64Key = _configuration.GetSection("Jwt:Secret").Value!;
      SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Convert.FromBase64String(base64Key));

      SigningCredentials credentials = new SigningCredentials(
              securityKey,
              SecurityAlgorithms.HmacSha512Signature);

      JwtSecurityToken token = new JwtSecurityToken(
          issuer: issuer,
          audience: audience,
          claims: claims,
          notBefore: DateTime.Now,
          expires: DateTime.Now.AddDays(1),
          signingCredentials: credentials
       );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
