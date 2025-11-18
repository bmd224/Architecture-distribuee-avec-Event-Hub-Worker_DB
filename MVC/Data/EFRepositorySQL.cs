using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepositorySQL : EFRepository<ApplicationDbContextSQL>
    {
        public EFRepositorySQL(ApplicationDbContextSQL context) : base(context) { }

        public override async Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize)
        {
            // Ajout d'un "order by", pour trier les resultats
            // Ajout d'un "take", pour prendre seulement une partie des entré, nous ferons une paginations plus tard.
            // Ajout d'un include pour ajouter a notre collection les commentaires lier a notre Post.

            return await _context.Posts.OrderByDescending(o => o.Created).Skip((pageNumber-1) * pageSize).Take(pageSize).Include(i => i.Comments).ToListAsync();
        }

    }
}
