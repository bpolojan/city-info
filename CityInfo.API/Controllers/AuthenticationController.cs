using CityInfo.API.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CityInfo.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            var user = ValidateUserCredentials(authenticationRequestBody.UserName, authenticationRequestBody.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            // Use the secret to create a key as Byte Array. Secret should be stored in a key vault or at least in Environment Variables
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));

            // SigningCredentials are used for signing the Token - Encrypt the securityKey - Sha256
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //In the Token we have Claims - identity related information on the user
            var claimsForToken = new List<Claim>();

            claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("city", user.City));

 //         JwtSecurityToken(string issuer = null, string audience = null,
 //                          IEnumerable <Claim> claims = null,
 //                          DateTime ? notBefore = null, DateTime ? expires = null,
 //                          SigningCredentials signingCredentials = null);

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler()
                .WriteToken(jwtSecurityToken);
            return Ok(tokenToReturn);
        }

        // Demo - will return always true
        private CityInfoUser ValidateUserCredentials(string? userName, string? password)
        {
            return new CityInfoUser(1, userName ?? "", "Bogdan", "Polojan", "Berlin");
        }
    }    
}

/* User Pass login - POST - user and pass in request Body - NOT via Query (requests might be logged)
 * 
 * Token based authentication 
 * Send the Token on each request
 * Validate the Token at API level * 
 * 
 * Create Authenticate EndPoint which checks on Auth request for (User, Pass) -> If valid return JWT as response
 * 
 *  Token:
 *  - Payload    - data - user name, city, etc
 *  - Signature  - Hash of the Payload. If someboy changes the paylod - the signature will not match anymore
 *               - Secret will be used to generate the SIGNING KEY. 
 *  - Header     - info about the token like the algorithm - SHA 256  
 *  
 *  Claims
 *  - What Info does the Token contain
 *  - Info about User ("given_name", "city") *  
 *  
 *  Ensure that the API can only be accesed with a valid Token
 *  
 *  Pass the token from Client to API as Bearer Token on each request
 *  Authorization: Bearer mytoken123
 */

// We can access in the request the User Claims and implement logic based on it
// User.Claims.FirstOrDefault(x => x.Type == "city")?.Value