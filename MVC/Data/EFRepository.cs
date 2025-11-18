using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public abstract class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        protected readonly TContext _context;

        protected EFRepository(TContext context) 
        { 
            _context = context; 
        }

        //Post
        public abstract Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize);
        public virtual async Task<int> GetPostsCount() { return await _context.Set<Post>().CountAsync(); }
        public virtual async Task Add(Post post) { _context.Add(post); await _context.SaveChangesAsync(); }

        public virtual async Task<Post?> GetPostById(Guid id)
        {
            return await _context.Set<Post>().FirstOrDefaultAsync(p => p.Id == id);
        }

        public virtual async Task IncrementPostLike(Guid id) { var post = await _context.Set<Post>().FindAsync(id); post!.IncrementLike(); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementPostDislike(Guid id) { var post = await _context.Set<Post>().FindAsync(id); post!.IncrementDislike(); await _context.SaveChangesAsync(); }

        //Comments
        public virtual async Task<List<Comment>> GetCommentsIndex(Guid id) { return await _context.Set<Comment>().Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync(); }
        public virtual async Task AddComments(Comment comment) { var post = await _context.Set<Post>().FindAsync(comment.PostId); post!.Comments.Add(comment); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementCommentLike(Guid id) { var comment = await _context.Set<Comment>().FindAsync(id); comment!.IncrementLike(); await _context.SaveChangesAsync(); }
        public virtual async Task IncrementCommentDislike(Guid id) { var comment = await _context.Set<Comment>().FindAsync(id); comment!.IncrementDislike(); await _context.SaveChangesAsync(); }
    }
}
