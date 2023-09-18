using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kvitretest.Models;
using Kvitretest.Services;

namespace Kvitretest.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public readonly IConfiguration Configuration;

        public readonly IDatabaseService DatabaseService;
        //public JsonUnsecureDatabaseService DatabaseService;
        //public SqliteDatabaseService DatabaseService;

        public IEnumerable<OneUser> IndexPageAllUsers { get; private set; }
        public IEnumerable<OnePost> IndexPageAllPosts { get; private set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
            //IDatabaseService databaseService
            //JsonUnsecureDatabaseService databaseService
            //SqliteDatabaseService databaseService
        )
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            else
            {
                _logger = logger;
            }

            string? conf;
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
                //conf = "";
            }
            else
            {
                Configuration = configuration;

                conf = Configuration.GetValue("Database", "");
                conf = (conf is null) ? "¿?¿" : conf.ToLower();
            }
            MyDebug.WriteLine("Wanted database:" + conf.ToString());

            if (conf == "sqlite")
            {
                DatabaseService = new SqliteDatabaseService(webHostEnvironment);
            }
            else if (conf == "json")
            {
                DatabaseService = new JsonUnsecureDatabaseService(webHostEnvironment);
            }
            else
            {
                DatabaseService = new JsonUnsecureDatabaseService(webHostEnvironment);

                MyDebug.WriteLine("ERROR! No recognised database:" + conf.ToString());
                //throw new ArgumentException("No recognised database service provided", "Configuration.Database");
            }

            IndexPageAllUsers = Enumerable.Empty<OneUser>();
            IndexPageAllPosts = Enumerable.Empty<OnePost>();
        }

        public void OnGet()
        {
            IndexPageAllUsers = DatabaseService.GetAllUsers();
            //IndexPageAllPosts = DatabaseService.GetAllPosts();
            IndexPageAllPosts = DatabaseService.GetAllPostsWithUsername();
            MyDebug.WriteLine("Hentet alle brukere og poster ... kanskje ...");
        }
    }
}
