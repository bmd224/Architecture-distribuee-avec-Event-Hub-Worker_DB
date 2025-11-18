using Microsoft.AspNetCore.Http.HttpResults;
using MVC.Models;

namespace MVC.Data
{
    public interface IRepositoryAPI
    {
        // API, avec implementation des DTO
        Task<Results<Ok<List<PostReadDTO>>, InternalServerError>> GetAPIPostsIndex();

        Task<Results<Ok<PostReadDTO>, NotFound, InternalServerError>> GetAPIPost(Guid id);

        Task<Results<Created<PostReadDTO>, BadRequest, InternalServerError>> CreateAPIPost(Post post);

        Task<Results<Ok, NotFound, InternalServerError>> APIIncrementPostLike(Guid id);

        Task<Results<Ok, NotFound, InternalServerError>> APIIncrementPostDislike(Guid id);

        Task<Results<Ok<List<CommentReadDTO>>, NotFound, InternalServerError>> GetAPIComment(Guid id);

        Task<Results<Created<CommentReadDTO>, NoContent, BadRequest, InternalServerError>> CreateAPIComment(CommentCreateDTO commentDTO);

        Task<Results<Ok, NotFound, InternalServerError>> APIIncrementCommentLike(Guid id);

        Task<Results<Ok, NotFound, InternalServerError>> APIIncrementCommentDislike(Guid id);


    }
}
