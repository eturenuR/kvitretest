﻿using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Text.Json;
using Kvitretest.Models;
using Kvitretest.Pages;
using Kvitretest.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;

namespace Kvitretest.Controllers
{
    [Route("api/v1/[controller]")]  // api/v1/posts
    [Route("api/[controller]")]     // api/posts
    [Route("[controller]")]         // posts
    [ApiController]
    public class PostsController : CommonPageController
    //public class PostsController : ControllerBase
    {
        private static readonly JsonSerializerOptions jsonDocumentWriteOptions = new()
        {
            WriteIndented = true
        };
        //private readonly IConfiguration Configuration;
        //private readonly IDatabaseService DataBaseService;

        public static readonly string ApiRootRouteStr = "/api/v1";

#pragma warning disable IDE0290 // Use primary constructor
        public PostsController(
#pragma warning restore IDE0290 // Use primary constructor
            ILogger<IndexModel> logger,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment) : base(
                logger, configuration, webHostEnvironment)
        {
            //DatabaseService = databaseService;
            //this.DatabaseService = databaseService;
            //Configuration = configuration;

            //var conf = Configuration.GetValue("Database", "");
            //conf = (conf is null) ? "" : conf.ToLower();
            //MyDebug.WriteLine(conf.ToString());
        }

        //Autentisert: NEI
        //Autorisert: NEI
        //Method: POST
        //URL: http://<BASE_URL>/api/v1/posts/all
        [HttpGet]
        [HttpPost]
        [Route("all")]
        //public IEnumerable<OnePost> Get()
        public ActionResult Get()
        {
            //return Enumerable.Empty<OnePost>();
            var allPosts = DatabaseService.GetAllPostsWithUsername();

            return Ok(JsonSerializer.Serialize(allPosts, jsonDocumentWriteOptions));  // 200
            //return DatabaseService.GetAllPostsWithUsername();

            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-7.0
        }

        //Autentisert: NEI
        //Autorisert: NEI
        //Method: POST
        //URL: http://<BASE_URL>/api/v1/posts/<id>
        [HttpGet("{PostId:int}")]
        [HttpPost("{PostId:int}")]
        //[Route("<id>")]
        public ActionResult Get([FromRoute] int PostId)
        {
            MyDebug.WriteLine(PostId);

            var res = DatabaseService.GetPost(PostId);

            if (res != null)
            {
                return Ok(res.ToString());
            }
            else
            {
                var returnHash = new Dictionary<string, string>()
                {
                    { "message", $"finner ikke posten  – {PostId}" },
                };
                return NotFound(returnHash);  // 404
                //return NoContent();  // 204
            }
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-7.0
        }

        //Autentisert: JA
        //Autorisert: NEI
        //Method: POST
        //URL: http://<BASE_URL>/api/v1/posts/create
        [HttpPost("Create")]
        public ActionResult Create(
            //[FromHeader(Name = "Content-Type")] string contentType,
            [FromHeader(Name = "Authorization"), Required] string AuthorizationToken,
            [FromForm(Name = "Message"), Required] string MessageText
            //[FromBody] string messageText  // Only one param can have FromBody
        )
        {
            //Console.WriteLine(contentType.ToString());
            Console.WriteLine(HttpContext.User.ToString());

            // Microsoft.AspNetCore.Http.DefaultHttpRequest
            //var re = Request;
            //Console.WriteLine(re.ToString());

            // Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestHeaders
            //var headers = re.Headers;
            //Console.WriteLine(headers.ToString());

            //int userId = -1;
            //string token = AuthorizationToken;
            //string decodedToken = UserAuth.DecodeBase64Token(AuthorizationToken);
            ////UserAuth authObj = new UserAuth(DatabaseService, decodedToken);
            //MyDebug.WriteLine(string.Format("id f>d: {0} -> {1} | t f>d: {2} -> {3} | m: {4}",
            //    userId, authObj.Id, token, decodedToken, MessageText));

            UserAuth authObj = new UserAuth(HttpContext.User);

            MyDebug.WriteLine(string.Format("id: {0} | au: {1} | t f>d: {2} -> {3} | m: {4}",
                authObj.Id, authObj.IsAuthenticated, AuthorizationToken, authObj.Token, MessageText));

            if (authObj.IsAuthenticated)
            {
                OnePost? postObj;
                Dictionary<string, string> returnHash;

                try
                {
                    postObj = DatabaseService.CreatePost(authObj.Id, MessageText);
                }
                catch (System.Data.DataException ex)
                {
                    MyDebug.WriteLine(ex.Message);
                    returnHash = new Dictionary<string, string>()
                    {
                        { "message", "opprette ny post feilet" },
                        { "error", ex.Message },
                    };
                    // TODO: Lag en ordentlig feilmeldingsside og en 500 eller 503 feilmelding
                    throw new Exception(JsonSerializer.Serialize(returnHash, jsonDocumentWriteOptions));
                }

                if (postObj != null)
                {
                    //string originalRouteUrlString = Request.GetDisplayUrl();
                    MyDebug.WriteLine(Request.GetDisplayUrl());

                    //var originalRouteUri = new Uri(originalRouteUrlString);
                    //string newRouteUrlString = string.Format("{0}://{1}:{2}{3}{4}",
                    //    originalRouteUri.Scheme,
                    //    originalRouteUri.DnsSafeHost, originalRouteUri.Port,
                    //    string.Join("", originalRouteUri.Segments, 0, (originalRouteUri.Segments.Length-1)),
                    //    postObj.Id.ToString()
                    //    );
                    //string newRouteUrlSegmentString = string.Join("", originalRouteUri.Segments,
                    //    0, (originalRouteUri.Segments.Length - 1)) + postObj.Id.ToString();

                    string[] arr = [ ApiRootRouteStr, "posts", postObj.Id.ToString() ];
                    string newRouteUrlSegmentString = string.Join('/', arr);
                    MyDebug.WriteLine(newRouteUrlSegmentString);

                    returnHash = new Dictionary<string, string>(){
                        { "message",      "opprettet" },
                        { "url",          newRouteUrlSegmentString },
                        { "post_id",      postObj.Id.ToString() },
                        { "post_user_id", (postObj.User_Id is null) ? "¿?¿" : postObj.User_Id.ToString() },
                        { "post_body",    (postObj.Body is null) ? "¿?¿" : postObj.Body.ToString() },
                    };
                    //var returnJson = JsonSerializer.Serialize(returnHash, jsonDocumentWriteOptions);

                    return Created(newRouteUrlSegmentString, returnHash);  // 201
                        //Content(returnJson,
                        //"application/json")
                        //);
                }
                else
                {
                    returnHash = new Dictionary<string, string>(){
                        { "message", "opprette ny post feilet" },
                    };
                    // TODO: Finn en bedre feilkode å sende. 500 eller noe.
                    return BadRequest(returnHash);  // 400
                }
            }
            else
            {
                MyDebug.WriteLine("Boooo!! Missing or not valid token !! Lag lag lag !!Boooo");

                // TODO: Vis med en bedre feilmelding at man ikke er logget inn / brukeren ikke har rettigheter
                var authItems = new Dictionary<string, string?>()
                {
                        { "message", "create urgh meehh" },
                };
                AuthenticationProperties authProps = new AuthenticationProperties(authItems);
                return Forbid(authProps);  // 403

                //string[] authSchemeArray = { "base64" };
                //return Forbid(authProps, authSchemeArray);
                //return Unauthorized("Boooo!! Lag lag lag !!Boooo");  // 401
            }
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-7.0
        }

        //Autentisert: JA
        //Autorisert: JA
        //Method: PUT
        //URL: http://<BASE_URL>/api/v1/posts/edit/<ID>
        //[HttpGet("Edit")]
        [HttpPost("Edit/{PostId:int}")]
        [HttpPost("{PostId:int}/Edit")]
        [HttpPut("Edit/{PostId:int}")]
        [HttpPut("{PostId:int}/Edit")]
        public ActionResult Edit(
            //[FromHeader(Name = "Content-Type")] string contentType,
            [FromHeader(Name = "Authorization"), Required] string AuthorizationToken,
            [FromForm(Name = "Message"), Required] string MessageText,
            //[FromBody] string messageText,  // Only one param can have FromBody
            [FromRoute(Name = "PostId")] int MessageId
        )
        {
            //var userId = -1;
            //var token = AuthorizationToken;
            //string decodedToken = UserAuth.DecodeBase64Token(AuthorizationToken);
            //UserAuth authObj = new UserAuth(DatabaseService, decodedToken);
            //MyDebug.WriteLine(string.Format("id f>d: {0} -> {1} | t f>d: {2} -> {3} | pid: {4} | m: {5}",
            //    userId, authObj.Id, token, decodedToken, MessageId, MessageText));

            UserAuth authObj = new UserAuth(HttpContext.User);

            MyDebug.WriteLine(string.Format("id: {0} | au: {1} | t f>d: {2} -> {3} | pid: {4} | m: {5}",
                authObj.Id, authObj.IsAuthenticated, AuthorizationToken, authObj.Token, MessageId, MessageText));

            OnePost? postToEdit = DatabaseService.GetPost(MessageId);
            Dictionary<string, string> returnHash;

            if (postToEdit != null)
            {
                if (authObj.CheckIsAuthorized(postToEdit.User_Id ?? "<null><¿?¿no user found¿?¿why¿?¿>"))
                {
                    postToEdit = DatabaseService.UpdatePost(MessageId, authObj.Id, MessageText);
                    if (postToEdit != null)
                    {
                        //string originalRouteUrlString = Request.GetDisplayUrl();
                        MyDebug.WriteLine(Request.GetDisplayUrl());

                        //var originalRouteUri = new Uri(originalRouteUrlString);
                        //string newRouteUrlString = string.Format("{0}://{1}:{2}{3}{4}",
                        //    originalRouteUri.Scheme,
                        //    originalRouteUri.DnsSafeHost, originalRouteUri.Port,
                        //    string.Join("", originalRouteUri.Segments, 0, (originalRouteUri.Segments.Length - 1)),
                        //    postToEdit.Id.ToString()
                        //    );
                        //string newRouteUrlSegmentString = string.Join("", originalRouteUri.Segments,
                        //    0, (originalRouteUri.Segments.Length - 1)) + postToEdit.Id.ToString();

                        string[] arr = [ApiRootRouteStr, "posts", "edit", postToEdit.Id.ToString()];
                        string newRouteUrlSegmentString = string.Join('/', arr);
                        MyDebug.WriteLine(newRouteUrlSegmentString);

                        returnHash = new Dictionary<string, string>(){
                            //{ "message", "endret" },
                            //{ "url", newRouteUrlSegmentString },
                            { "post_id", postToEdit.Id.ToString() },
                            { "post_user_id", (postToEdit.User_Id == null) ? "¿?¿" : postToEdit.User_Id.ToString() },
                            { "post_body", (postToEdit.Body == null) ? "¿?¿" : postToEdit.Body.ToString() },
                        };
                        var returnJson = JsonSerializer.Serialize(returnHash, jsonDocumentWriteOptions);

                        MyDebug.WriteLine(returnJson);
                        return Ok(returnJson);  // 200
                        //return NoContent();  // 204
                    }
                    else
                    {
                        returnHash = new Dictionary<string, string>(){
                            { "message", "endre post feilet" },
                            { "wanted_post_id", MessageId.ToString() },
                        };
                        // TODO: Finn en bedre feilkode å sende. 500 eller noe.
                        return BadRequest(returnHash);  // 400
                    }
                }
                else
                {
                    MyDebug.WriteLine("Boooo!! Missing or not valid token !! Endre endre endre !!Boooo");

                    // TODO: Vis med en bedre feilmelding at man ikke er logget inn / brukeren ikke har rettigheter
                    var authItems = new Dictionary<string, string?>()
                    {
                            { "message", "edit meehh blargh edit" },
                    };
                    AuthenticationProperties authProps = new AuthenticationProperties(authItems);
                    return Forbid(authProps);  // 403

                    //return Unauthorized("Booohooo!! Endre endre endre");  // 401
                }
            }
            else
            {
                returnHash = new Dictionary<string, string>()
                {
                    { "message", $"finner ikke posten  – {MessageId}" },
                };
                return NotFound(returnHash);  // 404
                //return NoContent();  // 204
            }
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-7.0
        }

        //Autentisert: JA
        //Autorisert: JA
        //Method: POST
        //URL: http://<BASE_URL>/api/v1/posts/delete/<ID>
        //[HttpGet("Delete")]
        [HttpPost("Delete/{PostId:int}")]
        [HttpPost("{PostId:int}/Delete")]
        public ActionResult Delete(
            [FromHeader(Name = "Authorization"), Required] string AuthorizationToken,
            [FromRoute(Name = "PostId")] int MessageId
        )
        {
            //var userId = -1;
            //var token = AuthorizationToken;
            //string decodedToken = UserAuth.DecodeBase64Token(AuthorizationToken);
            //UserAuth authObj = new UserAuth(DatabaseService, decodedToken);
            //MyDebug.WriteLine(string.Format("id f>d: {0} -> {1} | t f>d: {2} -> {3} | pid: {4}",
            //    userId, authObj.Id, token, decodedToken, MessageId));

            UserAuth authObj = new UserAuth(HttpContext.User);

            MyDebug.WriteLine(string.Format("id: {0} | au: {1} | t f>d: {2} -> {3} | pid: {4}",
                authObj.Id, authObj.IsAuthenticated, AuthorizationToken, authObj.Token, MessageId));

            OnePost? postToDelete = DatabaseService.GetPost(MessageId);
            Dictionary<string, string> returnHash;

            if (postToDelete != null)
            {
                if (authObj.CheckIsAuthorized(postToDelete.User_Id ?? "<null><¿?¿no user found¿?¿why¿?¿>"))
                {
                    bool success = DatabaseService.DeletePost(MessageId, authObj.Id);
                    if (success)
                    {
                        returnHash = new Dictionary<string, string>()
                        {
                            { "message", "slettet" },
                            //{ "post_id", postToDelete.Id.ToString() },
                            //{ "post_user_id", (postToDelete.User_Id == null) ? "¿?¿" : postToDelete.User_Id.ToString() },
                            //{ "user_id", authObj.Id.ToString() },
                        };
                    }
                    else
                    {
                        returnHash = new Dictionary<string, string>()
                        {
                            { "message", "ikke slettet" },
                            { "post_id", postToDelete.Id.ToString() },
                            { "post_user_id", (postToDelete.User_Id == null) ? "¿?¿" : postToDelete.User_Id.ToString() },
                            { "user_id", authObj.Id.ToString() },
                        };
                    }

                    // TODO: Bedre tilbakemelding.
                    var returnJson = JsonSerializer.Serialize(returnHash, jsonDocumentWriteOptions);

                    MyDebug.WriteLine(returnJson);
                    return Ok(returnJson);  // 200
                    //return NoContent();  // 204
                }
                else
                {
                    MyDebug.WriteLine("Boooo!! Missing or not valid token !! Slett slett slett !!Boooo");

                    // TODO: Vis med en bedre feilmelding at man ikke er logget inn / brukeren ikke har rettigheter
                    var authItems = new Dictionary<string, string?>()
                    {
                            { "message", "delete blargh delete" },
                    };
                    AuthenticationProperties authProps = new AuthenticationProperties(authItems);
                    return Forbid(authProps, "base64");  // 403

                    //return Unauthorized("Boooohooo!! Slett slett slett");  // 401
                }
            }
            else
            {
                // TODO: Bedre feilmelding.
                returnHash = new Dictionary<string, string>()
                {
                    { "message", $"kan ikke slette – finner ikke posten – {MessageId}" },
                    { "user_id", authObj.Id.ToString() },
                };
                var returnJson = JsonSerializer.Serialize(returnHash, jsonDocumentWriteOptions);

                MyDebug.WriteLine(returnJson);
                return Ok(returnJson);  // 200
                //return NoContent();  // 204
            }
            // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-7.0
        }
    }
}
