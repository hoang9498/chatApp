using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin.Auth;
using chatApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
namespace chatApp.Controllers
{
    public class LoginController : Controller
    {

        public LoginController()
        {
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); // show login form
        }

        [HttpPost]
        public async Task<ActionResult> UserLogin([FromBody] FirebaseLoginRequest request)
        {
            if (FirebaseAuth.DefaultInstance == null)
            {
                // diagnose the issue immediately in the logs
                return StatusCode(500, "Firebase Admin SDK not initialized.");
            }
            if (request == null || string.IsNullOrEmpty(request.IdToken))
            {
                return StatusCode(500,"Invalid request: IdToken is missing.");
            }
            
            // Set session expiration to 5 days.
            var options = new SessionCookieOptions()
            {
                ExpiresIn = TimeSpan.FromDays(5),
            };
            try
            {
                // To ensure that cookies are set only on recently signed in users
                // check auth_time in
                // ID token before creating a cookie.
                var decodedToken = await FirebaseAuth.DefaultInstance
                                    .VerifyIdTokenAsync(request.IdToken);
                // store session cookie for image upload by using FirebaseStorage.net
                // place after decodedToken because we need to verify token first
                // if token is invalid, exception will be thrown before this line
                HttpContext.Session.SetString("FirebaseToken", request.IdToken);
                var authTime = new DateTime(1970, 1, 1).AddSeconds(
                (long)decodedToken.Claims["auth_time"]);
                if (DateTime.UtcNow - authTime < TimeSpan.FromMinutes(5))
                {
                    
                        // Create the session cookie.
                        //  This will also verify the ID token in the process.
                        // The session cookie will have the same claims as the ID token.
                        var sessionCookie = await FirebaseAuth.DefaultInstance
                            .CreateSessionCookieAsync(request.IdToken, options);

                        // Set cookie policy parameters as required.
                        var cookieOptions = new CookieOptions()
                        {
                            Expires = DateTimeOffset.UtcNow.Add(options.ExpiresIn),
                            HttpOnly = true,
                            Secure = true,
                        };
                        Response.Cookies.Append("session", sessionCookie, cookieOptions);
                        // 2. Create claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, decodedToken.Uid),
                            new Claim(ClaimTypes.Name, decodedToken.Uid)
                        };

                        var identity = new ClaimsIdentity(
                            claims,
                            CookieAuthenticationDefaults.AuthenticationScheme
                        );

                        var principal = new ClaimsPrincipal(identity);

                        // 3. ISSUE AUTH COOKIE
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal
                        );
                        return Ok(new { success = true });    
                }
                else
                {
                    return Unauthorized("Recent sign-in required");
                }
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException)
            {
                return Unauthorized("Failed to create a session cookie");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            var sessionCookie = Request.Cookies["session"];
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifySessionCookieAsync(sessionCookie);
                await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(decodedToken.Uid);
                Response.Cookies.Delete("session");
                // Clear the existing external cookie
                await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
                return Ok();
            }
            catch (FirebaseAuthException)
            {
                return Redirect("/Login/UserLogin");
            }
        }


    }
}