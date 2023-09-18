using Microsoft.AspNetCore.Mvc;
using Kvitretest.Pages;
using Kvitretest.Services;

namespace Kvitretest.Controllers
{
    public class CommonPageController : ControllerBase
    {
        /// <summary>
        /// Reference to the log.
        /// </summary>
        private readonly ILogger<IndexModel> _logger;
        /// <summary>
        /// Reference to the startup/runtime Configuration settings.
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// Reference to the database service that is specified in the configuration.
        /// </summary>
        public readonly IDatabaseService DatabaseService;
        //public SqliteDatabaseService DatabaseService { get; }
        //public JsonUnsecureDatabaseService DatabaseService { get; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="webHostEnvironment"></param>
        public CommonPageController(
            ILogger<IndexModel> logger,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
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
                MyDebug.WriteLine("ERROR! No recognised database:" + conf.ToString());

                DatabaseService = new JsonUnsecureDatabaseService(webHostEnvironment);
                //throw new ArgumentException("No recognised database service provided", "Configuration.Database");
            }
            //DatabaseService = _databaseService;
        }
    }
}
