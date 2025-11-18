using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MVC.Models;
using MVC.Business;
using MVC.Data;
using Microsoft.Extensions.Logging;

namespace MVC.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        // Dépendances
        private readonly IRepository _repo;
        private readonly BlobController _blobController;
        private readonly ServiceBusController _serviceBusController;
        private readonly EventHubController _eventHubController;
        private readonly ILogger<PostsController> _logger;

        // Constructeur
        public PostsController(
            IRepository repo,
            BlobController blobController,
            ServiceBusController serviceBusController,
            EventHubController eventHubController,
            ILogger<PostsController> logger)
        {
            _repo = repo;
            _blobController = blobController;
            _serviceBusController = serviceBusController;
            _eventHubController = eventHubController;
            _logger = logger;
        }

        // Affichage des posts
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var posts = await _repo.GetPostsIndex(pageNumber, pageSize);
            var totalPosts = await _repo.GetPostsCount();
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            var viewModel = new PostIndexViewModel
            {
                Posts = posts,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // Affiche le formulaire de création de post
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Traitement du formulaire de post avec image
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Category,User")] Post post, IFormFile fileToUpload)
        {
            // Vérifie qu’un fichier a bien été envoyé
            if (fileToUpload == null || fileToUpload.Length == 0)
            {
                ModelState.AddModelError("fileToUpload", "Veuillez sélectionner un fichier.");
                return View(post);
            }

            try
            {
                // Génère les identifiants pour le post et le blob
                Guid postId = Guid.NewGuid();
                Guid blobId = Guid.NewGuid();

                // Envoie de l'image dans le Blob Storage
                string url = await _blobController.PushImageToBlob(fileToUpload, blobId);

                post.Id = postId;
                post.BlobImage = blobId;
                post.Url = url;
                post.Created = DateTime.Now;
                post.IsApproved = null;

                // Enregistrement
                await _repo.Add(post);
                // Envoi de messages vers Service Bus pour le resize et validation
                await _serviceBusController.SendImageToResize(blobId, postId);
                await _serviceBusController.SendContentImageToValidation(blobId, Guid.Empty, postId);
                // Envoi de l'événement vers Event Hub
                await _eventHubController.SendEventAsync(new Event(post));

                TempData["Notification"] = "Post en cours de validation...";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                // Gestion des exceptions liées au traitement du fichier
                ModelState.AddModelError("fileToUpload", "Erreur lors du traitement du fichier.");
                return View(post);
            }
        }

        // Incrémente les likes d’un post et revalidation
        [HttpPost]
        public async Task<IActionResult> Like(Guid id)
        {
            var post = await _repo.GetPostById(id);
            if (post == null || post.BlobImage == null)
            {
                TempData["Error"] = "Post ou image manquante";
                return RedirectToAction(nameof(Index));
            }

            await _repo.IncrementPostLike(id);

            try
            {
                //await _serviceBusController.SendContentImageToValidation(post.BlobImage.Value, id);
                // Validation du contenu image
                await _serviceBusController.SendContentImageToValidation(post.BlobImage.Value, Guid.Empty, id);

                TempData["Notification"] = "Re-validation déclenchée";
                _logger.LogInformation($"Re-validation envoyée pour Post {id} (Blob: {post.BlobImage})");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Échec de l'envoi à la validation";
                _logger.LogError(ex, $"Échec SendContentImageToValidation pour Post {id}");
            }

            return RedirectToAction(nameof(Index));
        }

        // Incrémente les dislikes d’un post
        [HttpPost]
        public async Task<IActionResult> Dislike(Guid id)
        {
            await _repo.IncrementPostDislike(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
