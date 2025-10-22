namespace Blog.Mcv.Models
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}
