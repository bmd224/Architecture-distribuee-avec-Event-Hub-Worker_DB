using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepositoryInMemory : EFRepository<ApplicationDbContextInMemory>
    {
        public EFRepositoryInMemory(ApplicationDbContextInMemory context) : base(context) { }
       
        public override async Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize) { return await _context.Posts.OrderByDescending(o => o.Created).Skip((pageNumber - 1) * pageSize).Take(pageSize).Include(i => i.Comments).ToListAsync(); }
        public override async Task<List<Comment>> GetCommentsIndex(Guid postId) { return await _context.Comments.Where(w => w.PostId == postId).OrderBy(o => o.Created).ToListAsync(); }
    }
}
