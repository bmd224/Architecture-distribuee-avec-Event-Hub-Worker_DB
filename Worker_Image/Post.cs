using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Worker_Image.Models;

namespace Worker_Image.Models
{
    public enum Category
    {
        Humour = 0,
        Nouvelle = 1,
        Inconnue = 2,
    }

    [NotMapped]
    public class PostForm : Post
    {
        [Required(ErrorMessage = "Un fichier est requis, il doit être de 20 MB et moins")]
        public required IFormFile FileToUpload { get; set; }
    }

    [NotMapped]
    public class PostIndexViewModel
    {
        public List<Post> Posts { get; set; } = new List<Post>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "SVP entrer un titre que personne ne lira")]
        [MaxLength(128)]
        [Display(Name = "Titre")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "SVP entrer une catégorie")]
        [EnumDataType(typeof(Category))]
        [Display(Name = "Catégorie")]
        public Category Category { get; set; }

        [Required(ErrorMessage = "SVP entrer un nom d'utilisateur")]
        [MaxLength(128)]
        [Display(Name = "Nom de l'utilisateur")]
        public required string User { get; set; }

        [Display(Name = "Like")]
        public int Like { get; private set; } = 0;

        [Display(Name = "Dislike")]
        public int Dislike { get; private set; } = 0;

        [Display(Name = "Date de création")]
        public DateTime Created { get; set; } = DateTime.Now;

        [Display(Name = "Contenue revisé ?")]
        public bool? IsApproved { get; private set; } = null;

        public bool IsDeleted { get; private set; } = false;

        public required Guid? BlobImage { get; set; }

        public required string Url { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public override string ToString()
        {
            return $"===============" + Environment.NewLine +
                $"Title : {Title}" + Environment.NewLine +
                $"Category : {Category}" + Environment.NewLine +
                $"User : {User}" + Environment.NewLine +
                $"Like : {Like}" + Environment.NewLine +
                $"Dislike : {Dislike}" + Environment.NewLine +
                $"Created : {Created}" + Environment.NewLine +
                $"===============";
        }

        public void IncrementLike()
        {
            Like++;
        }

        public void IncrementDislike()
        {
            Dislike++;
        }

        public void Approve()
        {
            IsApproved = true;
        }

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}
