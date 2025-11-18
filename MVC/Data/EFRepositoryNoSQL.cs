using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepositoryNoSQL : EFRepository<ApplicationDbContextNoSQL>
    {
        public EFRepositoryNoSQL(ApplicationDbContextNoSQL context) : base(context) { }

        public override async Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize)
        {
            List<Post> posts = await _context.Posts
                .OrderByDescending(o => o.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<Guid> postIds = posts.Select(p => p.Id).ToList();
            List<Comment> comments = await _context.Comments
                .Where(c => postIds.Contains(c.PostId))
                .ToListAsync();

            foreach (var post in posts)
            {
                post.Comments = comments.Where(w => w.PostId == post.Id).ToList();
            }

            return posts;
        }

        public override async Task<List<Comment>> GetCommentsIndex(Guid postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }

        public override async Task AddComments(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public override async Task IncrementCommentLike(Guid commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment != null)
            {
                comment.IncrementLike();
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
            }
        }

        public override async Task IncrementCommentDislike(Guid commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment != null)
            {
                comment.IncrementDislike();
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
            }
        }

        public override async Task IncrementPostLike(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post != null)
            {
                post.IncrementLike();
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }

        public override async Task IncrementPostDislike(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post != null)
            {
                post.IncrementDislike();
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}

