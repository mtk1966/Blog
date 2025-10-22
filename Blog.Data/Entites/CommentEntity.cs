using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Data.Entites
{
    public class CommentEntity : BaseEntity
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int BlogId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(BlogId))]
        public BlogsEntity Blog { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; } = null!;
    }
}
