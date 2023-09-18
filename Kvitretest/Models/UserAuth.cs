using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Kvitretest.Services;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Kvitretest.Models
{
    public class UserAuth
    {
        private readonly IDatabaseService? _databaseService;
        public int Id { get; private set; }
        public string Token { get; private set; }

        public bool IsAuthenticated { get; private set; }
        public bool IsAuthorized { get; private set; }

        public UserAuth()
        {
            _databaseService = null;
            Id = -1;
            Token = string.Empty;

            throw new Exception("Why did this happen?");
        }

        public UserAuth(IDatabaseService databaseService, string token)
        {
            _databaseService = databaseService;
            Token = token;
            Id = _databaseService.GetUserIdFromToken(token);

            IsAuthenticated = (Id > 0);
            //IsAuthorized = (Id > 0);
            IsAuthorized = false;
        }

        public UserAuth(IDatabaseService databaseService, int id, string token)
        {
            _databaseService = databaseService;
            Id = id;
            Token = token;

            int verifyId = _databaseService.GetUserIdFromToken(token);

            IsAuthenticated = (Id > 0 && Id == verifyId);
            //IsAuthorized = (Id > 0 && Id == verifyId);
            IsAuthorized = false;
        }

        public UserAuth(System.Security.Claims.ClaimsPrincipal claimObj)
        {
            if (claimObj is null || claimObj.Identity is null || claimObj.Identity.Name is null)
            {
                //_databaseService = null;
                Id = -1;
                Token = "";
                IsAuthenticated = false;
                IsAuthorized = false;
                return;
            }

            // Decoded token should be the first value that was inserted into the claims list.
            string headerToken = claimObj.Claims.First().Value;
            string headerUserIdStr = claimObj.Identity.Name ?? "";
            int headerUserId;
            try
            {
                headerUserId = Int32.Parse(headerUserIdStr);
            }
            catch (FormatException ex)
            {
                MyDebug.WriteLine(ex.Message);

                //_databaseService = null;
                Id = -1;
                Token = "";
                IsAuthenticated = false;
                IsAuthorized = false;
                return;
            }

            Id = headerUserId;
            //Token = claimObj.Identity.Label ?? "";  // can't be accessed ...?
            Token = headerToken;
            IsAuthenticated = true;
            IsAuthorized = false;
        }

        public bool CheckIsAuthorized(string postUserId)
        {
            return (IsAuthorized = (Id > 0 && Id.ToString() == postUserId));
            //return IsAuthorized;
        }

        public static string EncodeBase64Token(string token)
        {
            // 1111111111  ->  MTExMTExMTExMQ==
            // 2222222222  ->  MjIyMjIyMjIyMg==
            // 3333333333  ->  MzMzMzMzMzMzMw==
            byte[] plainTextBytes = [];

            if (token != null && token.Length > 0)
            {
                plainTextBytes = System.Text.Encoding.UTF8.GetBytes(token);
            }
            MyDebug.WriteLine($"{token} -> {plainTextBytes}");

            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string DecodeBase64Token(string base64Token)
        {
            // 1111111111  ->  MTExMTExMTExMQ==
            // 2222222222  ->  MjIyMjIyMjIyMg==
            // 3333333333  ->  MzMzMzMzMzMzMw==
            byte[] base64EncodedBytes = [];

            //MyDebug.WriteLine(base64Token);

            if (base64Token != null && base64Token.StartsWith("Basic "))
            {
                var tokens = base64Token.Split(' ', 2);
                base64Token = (tokens[1] ?? "").Trim();
            }
            else
            {
                return "";
            }

            if (base64Token != null && base64Token.Length > 0)
            {
                try
                {
                    base64EncodedBytes = System.Convert.FromBase64String(base64Token);
                }
                catch (FormatException ex)
                {
                    MyDebug.WriteLine(base64Token);
                    MyDebug.WriteLine(ex.Message);
                }
            }
            //MyDebug.WriteLine($"{base64Token} -> {base64EncodedBytes}");

            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string ParseAuthenticationHeaders(IHeaderDictionary headers, string headerKey)
        {
            //string authHeaderToken = "";

            if (headers.TryGetValue(headerKey, out StringValues headerValues))
            {
                string authHeaderToken = headerValues.ToString();
                MyDebug.WriteLine(authHeaderToken);

                //string possibleAuthToken = DecodeBase64Token(authHeaderToken);
                //return possibleAuthToken;
                return DecodeBase64Token(authHeaderToken);
            }

            return "";
        }
    }

    public class MyAuthOptions : AuthenticationSchemeOptions
    {
    }

    public class MyAuthenticationHandler : AuthenticationHandler<MyAuthOptions>
    {
        private readonly IDatabaseService? _databaseService;

        public MyAuthenticationHandler(
            IDatabaseService databaseService,
            IOptionsMonitor<MyAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Currently does nothing.
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            //var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            //var username = credentials[0];
            //var password = credentials[1];
            //userObj = await userservice.Authenticate(username, password);

            //if (userObj == null)
            //    return AuthenticateResult.Fail("Invalid Credentials");

            string base64Token = UserAuth.ParseAuthenticationHeaders(Request.Headers, "Authorization");

            return ValidateToken(base64Token);
        }

        private AuthenticateResult ValidateToken(string token)
        {
            string convertedToken = token;
            UserAuth? userAuthObj = _databaseService is null ? null : new UserAuth(_databaseService, convertedToken);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Authentication, convertedToken),
                new Claim(ClaimTypes.Name,           userAuthObj is null ? "" : userAuthObj.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userAuthObj is null ? "" : userAuthObj.Id.ToString()),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            identity.Label = convertedToken;
            var principal = new System.Security.Principal.GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            if (userAuthObj != null && userAuthObj.Id > 0)
            {
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("no such user");
            }
        }
    }
}
