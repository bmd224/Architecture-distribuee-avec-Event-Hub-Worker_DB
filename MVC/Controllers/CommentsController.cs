using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Business;
using MVC.Data;
using MVC.Models;


namespace MVC.Controllers
{
    //Utilisateur authentifies seulement 
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly IRepository _repo;
        private readonly ServiceBusController _serviceBusController;
        private readonly EventHubController _eventHubController;
       
        //Constructeur
        public CommentsController(IRepository repo, ServiceBusController serviceBusController, EventHubController eventHubController)
        {
            _repo = repo;
            _serviceBusController = serviceBusController;
            _eventHubController = eventHubController;
        }

        // Affiche la liste des commentaires liés à un post donné
        public async Task<IActionResult> Index(Guid id)
        {
            return View(await _repo.GetCommentsIndex(id));
        }

        // Formulaire de création de commentaire
        [HttpGet]
        public IActionResult Create(Guid PostId)
        {
            ViewData["PostId"] = PostId;
            return View();
        }
        // Lorsqu'on fait la soumission du formulaire de commentaire
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Commentaire,User,PostId")] Comment comment)
        {
            ModelState.Remove("Post");

            // Si le modèle est invalide réaffiche le formulaire avec les erreurs
            if (!ModelState.IsValid)
            {
                ViewData["PostId"] = comment.PostId;
                return View(comment); // Réaffiche le formulaire avec les erreurs
            }

            // Génère un ID pour les commentaire et ajoute les métadonnées
            comment.Id = Guid.NewGuid();
            comment.Created = DateTime.Now;
            comment.IsApproved = null; 

            //Ajoute le commentaire
            await _repo.AddComments(comment);
            // Envoie le contenu pour validation via Azure Service Bus
            await _serviceBusController.SendContentTextToValidation(comment.Commentaire, comment.Id, comment.PostId);
            //Envoie un evenement vers Event Hub
            await _eventHubController.SendEventAsync(new Event(comment));

            TempData["Notification"] = "Commentaire en cours de validation...";
            return RedirectToAction(nameof(Index), new { id = comment.PostId });
        }

        // Incrémente le nombre de likes d’un commentaire
        [HttpPost]
        public async Task<IActionResult> Like(Guid CommentId, Guid PostId)
        {
            await _repo.IncrementCommentLike(CommentId);
            return RedirectToAction(nameof(Index), new { id = PostId });
        }

        // Incrémente le nombre de dislikes d'un commentaire
        [HttpPost]
        public async Task<IActionResult> Dislike(Guid CommentId, Guid PostId)
        {
            await _repo.IncrementCommentDislike(CommentId);
            return RedirectToAction(nameof(Index), new { id = PostId });
        }
    }
}
