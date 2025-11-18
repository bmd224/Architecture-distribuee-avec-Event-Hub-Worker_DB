using MVC.Models;
using System;

namespace API.Models
{
    [Serializable]
    public class Event
    {
        public MediaType MediaType { get; set; }
        public EventAction Action { get; set; }

        public Guid PostId { get; set; }
        public Guid? CommentId { get; set; }

        public required string Data { get; set; }

        // Constructeur par defaut 
        public Event() { }

        // Constructeur 
        public Event(MediaType mediaType, EventAction action, Guid postId, string data = "", Guid commentId = default)
        {
            MediaType = mediaType;
            Action = action;
            PostId = postId;
            CommentId = commentId;
            Data = data;
        }

        // Constructeur pour les commentaires
        public Event(Comment commentaire)
            : this(MediaType.Text, EventAction.Submitted, commentaire.PostId, commentaire.Commentaire, commentaire.Id)
        {
        }

        // Constructeur pour les posts
        public Event(Post post)
            : this(MediaType.Image, EventAction.Submitted, post.Id, post.BlobImage?.ToString() ?? "")
        {
        }

        // Constructeur pour transformer un événement (ex: Validated, Refused, )
        public Event(MediaType mediaType, EventAction action, Event sourceEvent)
        {
            MediaType = mediaType;
            Action = action;
            PostId = sourceEvent.PostId;
            Data = sourceEvent.Data;
            CommentId = sourceEvent.CommentId;
        }
    }

    public enum MediaType
    {
        Image = 0,
        Text = 1,
    }

    public enum EventAction
    {
        Submitted,
        Resized,
        Validated,
        Refused,
        Deleted
    }
}
