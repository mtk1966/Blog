using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Data.Entites
{
    public class BlogsEntity : BaseEntity
    {
        [Required,MaxLength(70)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public int UserId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = null!;

        public ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
    }
}
