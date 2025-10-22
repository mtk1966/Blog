using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Data.Entites
{
    public class UserEntity : BaseEntity
    {
        [Required,MaxLength(100),EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required,MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required,MaxLength(100),DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public int RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public RoleEntity Role { get; set; } = default!;

    }
}
