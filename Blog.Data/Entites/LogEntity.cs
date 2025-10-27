using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Data.Entites
{
    [Table("Logs")]
    public class LogEntity
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public DateTime? TimeStamp { get; set; }
        public string Exception { get; set; } = string.Empty;
        public string Properties { get; set; } = string.Empty; // Stored as XML or JSON
    }
}