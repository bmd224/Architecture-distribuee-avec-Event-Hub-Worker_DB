namespace MVC.Models
{

    [Serializable]
    public class ContentTypeValidation
    {
        public ContentType ContentType { get; set; }
        public string Content { get; set; }
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }

        public ContentTypeValidation(ContentType contentType, string content, Guid commentId, Guid postId)
        {
            ContentType = contentType;
            Content = content;
            CommentId = commentId;
            PostId = postId;
        }
    }

    public enum ContentType
    {
        Image = 0,
        Text = 1,
    }
}
