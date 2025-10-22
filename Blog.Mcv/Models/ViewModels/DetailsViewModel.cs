using Blog.Data.Entites;

namespace Blog.Mcv.Models
{
    public class DetailsViewModel
    {
        public BlogsEntity Blog { get; set; } = null!;
        public ICollection<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
    }
}
