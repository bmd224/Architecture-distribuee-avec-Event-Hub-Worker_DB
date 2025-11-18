using System;

namespace MVC.Models
{
    [Serializable]
    public class Event
    {
        public MediaType MediaType { get; set; }
        public EventAction Action { get; set; }
        public Guid PostId { get; set; }
        public Guid? CommentId { get; set; }
        public string Data { get; set; } = string.Empty;

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

        // Constructeur pour un commentaire
        public Event(Comment commentaire)
        {
            MediaType = MediaType.Text;
            Action = EventAction.Submitted;
            PostId = commentaire.PostId;
            CommentId = commentaire.Id;
            Data = commentaire.Commentaire;
        }

        // Constructeur pour un post image
        public Event(Post post)
        {
            MediaType = MediaType.Image;
            Action = EventAction.Submitted;
            PostId = post.Id;
            CommentId = null;
            Data = post.BlobImage?.ToString() ?? string.Empty;
        }

        // Constructeur pour transformer un événement existant Validated, ..
        public Event(MediaType mediaType, EventAction action, Event sourceEvent)
        {
            MediaType = mediaType;
            Action = action;
            PostId = sourceEvent.PostId;
            CommentId = sourceEvent.CommentId;
            Data = sourceEvent.Data;
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
