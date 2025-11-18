using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Worker_Content.Models;

namespace Worker_Content.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } // modifié de init à set

        [Required(ErrorMessage = "SVP entrer votre commentaire")]
        [MaxLength(128)]
        [Display(Name = "Commentaire")]
        public string Commentaire { get; set; } = string.Empty;

        [Required(ErrorMessage = "SVP entrer un nom d'utilisateur")]
        [MaxLength(128)]
        [Display(Name = "Nom de l'utilisateur")]
        public string User { get; set; } = string.Empty;

        [Display(Name = "Like")]
        public int Like { get; private set; } = 0;

        [Display(Name = "Dislike")]
        public int Dislike { get; private set; } = 0;

        [Display(Name = "Date de création")]
        public DateTime Created { get; set; } = DateTime.Now; 

        [Display(Name = "Contenue revisé ?")]
        public bool? IsApproved { get; set; } = false;

        public bool IsDeleted { get; private set; } = false;

        public Post Post { get; set; } = null!;

        public Guid PostId { get; set; } // modifié de init à set

        public override string ToString()
        {
            return $"===============" + Environment.NewLine +
                   $"Comment : {Commentaire}" + Environment.NewLine +
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
