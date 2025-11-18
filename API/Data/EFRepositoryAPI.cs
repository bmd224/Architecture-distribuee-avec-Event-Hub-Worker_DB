using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public abstract class EFRepositoryAPI<TContext> : IRepositoryAPI where TContext : DbContext
    {
        protected readonly TContext _context;

        protected EFRepositoryAPI(TContext context)
        {
            _context = context;
        }

        //API
        // Avec l'implementation du DTO
        public virtual async Task<Results<Ok<List<PostReadDTO>>, InternalServerError>> GetAPIPostsIndex()
        {
            try
            {
                // Converstion dans le DTO
                Post[] posts = await _context.Set<Post>().OrderByDescending(o => o.Created).ToArrayAsync();
                List<PostReadDTO> postsDTO = posts.Select(x => new PostReadDTO(x)).ToList();

                return TypedResults.Ok(postsDTO);
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok<PostReadDTO>, NotFound, InternalServerError>> GetAPIPost(Guid id)
        {
            try
            {
                var post = await _context.Set<Post>().FirstOrDefaultAsync(w => w.Id == id);
                if (post == null)
                    return TypedResults.NotFound();
                else
                    return TypedResults.Ok(new PostReadDTO(post));
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        // Normallement cette méthode ne devrait pas être Post, cette object est interne, mais nous avons géré la conversion dans une autre méthode interne avant ...
        public virtual async Task<Results<Created<PostReadDTO>, BadRequest, InternalServerError>> CreateAPIPost(Post post)
        {
            try
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return TypedResults.Created($"/Posts/{post.Id}", new PostReadDTO(post));

            }
            catch (Exception ex) when (ex is DbUpdateException)
            {
                return TypedResults.BadRequest();
            }
            catch (Exception)
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementPostLike(Guid id)
        {
            try
            {
                var post = await _context.Set<Post>().FindAsync(id);
                if (post == null)
                    return TypedResults.NotFound();
                else
                {
                    post!.IncrementLike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementPostDislike(Guid id)
        {
            try
            {
                var post = await _context.Set<Post>().FindAsync(id);
                if (post == null)
                    return TypedResults.NotFound();
                else
                {
                    post!.IncrementDislike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok<List<CommentReadDTO>>, NotFound, InternalServerError>> GetAPIComment(Guid id)
        {
            try
            {
                Comment[] comments = await _context.Set<Comment>().Where(x => x.Id == id || x.PostId == id).OrderByDescending(o => o.Created).ToArrayAsync();
                if (comments.Length > 0)
                {
                    // Converstion dans le DTO
                    List<CommentReadDTO> commentsDTO = comments.Select(x => new CommentReadDTO(x)).ToList();
                    return TypedResults.Ok(commentsDTO);
                }  
                return TypedResults.NotFound();
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Created<CommentReadDTO>, NoContent, BadRequest, InternalServerError>> CreateAPIComment(CommentCreateDTO commentDTO)
        {
            try
            {
                var post = await _context.Set<Post>().FindAsync(commentDTO.PostId);
                if (post == null)
                    return TypedResults.NoContent();
                else
                {
                    Comment comment = CommentCreateDTO.GetComment(commentDTO);
                    post!.Comments.Add(comment);
                    await _context.SaveChangesAsync();
                    return TypedResults.Created($"/Comments/{comment.Id}", new CommentReadDTO(comment));
                }
            }
            catch (Exception ex) when (ex is DbUpdateException)
            {
                return TypedResults.BadRequest();
            }
            catch (Exception)
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementCommentLike(Guid id)
        {
            try
            {
                var Comment = await _context.Set<Comment>().FindAsync(id);
                if (Comment == null)
                    return TypedResults.NotFound();
                else
                {
                    Comment!.IncrementLike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok, NotFound, InternalServerError>> APIIncrementCommentDislike(Guid id)
        {
            try
            {
                var Comment = await _context.Set<Comment>().FindAsync(id);
                if (Comment == null)
                    return TypedResults.NotFound();
                else
                {
                    Comment!.IncrementDislike();
                    await _context.SaveChangesAsync();
                    return TypedResults.Ok();
                }
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

    }
}
