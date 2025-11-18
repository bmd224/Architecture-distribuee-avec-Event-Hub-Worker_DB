using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;

namespace MVC.Models
{

    /// <summary>
    /// Implementation de la class DTO pour la lecture des Post
    /// </summary>
    public class PostReadDTO
    {
        /// <summary>
        /// ID du Post
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Titre du Post
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Catégorie du Post
        /// </summary>
        public Category Category { get; init; }

        /// <summary>
        /// Usager qui a soumis le Post
        /// </summary>
        public string? User { get; init; }

        /// <summary>
        /// Nombre de Like
        /// </summary>
        public int Like { get; init; }

        /// <summary>
        /// Nombre de Dislike
        /// </summary>
        public int Dislike { get; init; }

        /// <summary>
        /// Date de création
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// URL de l'image du Post
        /// </summary>
        public string? Url { get; init; }

        // Constructeur
        public PostReadDTO() { }

        public PostReadDTO(Post post)
        {
            Id = post.Id;
            Title = post.Title;
            Category = post.Category;
            User = post.User;
            Like = post.Like;
            Dislike = post.Dislike;
            Created = post.Created;
            Url = post.Url;
        }
    }

    public class PostCreateDTO
    {
        [FromForm(Name = "Title")]
        public string? Title { get; init; }

        [FromForm(Name = "Category")]
        public Category Category { get; init; }

        [FromForm(Name = "User")]
        public string? User { get; init; }

        [FromForm(Name = "Image")]
        public IFormFile? Image { get; init; }

        public PostCreateDTO() { }

        // Ajout d'un constructeur pour les API
        public PostCreateDTO(string Title, string Category, string User, IFormFile Image)
        {
            this.Title = Title;
            this.Category = (Category)Enum.Parse(typeof(Category), Category);
            this.User = User;
            this.Image = Image;
        }

        public PostCreateDTO(string Title, Category Category, string User, IFormFile Image)
        {
            this.Title = Title;
            this.Category = Category;
            this.User = User;
            this.Image = Image;
        }
    }
}
