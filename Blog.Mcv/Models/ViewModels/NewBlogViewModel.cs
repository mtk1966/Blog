using System.ComponentModel.DataAnnotations;

namespace Blog.Mcv.Models
{
    public class NewBlogViewModel
    {
        [Required, MaxLength(100)]
        public string Title { get; set; } = null!;
        [Required, DataType(DataType.Html)]
        public string Content { get; set; } = null!;
    }
}
