using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

// Requis pour l'injection de la dépendance pour l'AppConfig
using Microsoft.Extensions.Options;

// Requis pour l'injection de la dépendance pour le Flag Management
using Microsoft.FeatureManagement;
using System.Security.Claims;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Configuration pour recevoir les ApplicationConfiguration du AppConfig ...
        private ApplicationConfiguration _applicationConfiguration { get; }

        // Configuration pour recevoir les Flags
        private IFeatureManager _featureManager { get; }

        // Voir le IOptionsSnapshot qui importe dans l'object la configuraiton du AppConfig.
        // Ainsi que le IFeatureManager pour la gestion des Flags
        public HomeController(ILogger<HomeController> logger, IOptionsSnapshot<ApplicationConfiguration> options, IFeatureManager featureManager)
        {
            _applicationConfiguration = options.Value;
            _logger = logger;
            _featureManager = featureManager;
        }

        public IActionResult Index()
        {
            return View(_applicationConfiguration);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static UserAzureAD GetUserOnAzureAd(ClaimsPrincipal user)
        {
            var preferredUsernameClaim = user.Claims.FirstOrDefault(c => c.Type.Equals("preferred_username"));
            if (preferredUsernameClaim != null)
            {
                return new UserAzureAD
                {
                    user_name = user.Claims.FirstOrDefault(p => p.Type.Equals("name"))!.Value,
                    user_email = preferredUsernameClaim.Value,
                    user_domain = string.Format(@"Domain\{0}", preferredUsernameClaim.Value.Split('@')[0])
                };
            }
            return null!;
        }
    }
}
