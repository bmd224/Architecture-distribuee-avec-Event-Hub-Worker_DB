using System;

namespace Worker_DB.Models
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
        //Constructeur avec param
        public Event(MediaType mediaType, EventAction action, Guid postId, string data = "", Guid? commentId = null)
        {
            MediaType = mediaType;
            Action = action;
            PostId = postId;
            Data = data;
            CommentId = commentId;
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
