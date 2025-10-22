namespace Blog.Mcv.Models
{
    public class BlogTableViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string BlogOwner { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
