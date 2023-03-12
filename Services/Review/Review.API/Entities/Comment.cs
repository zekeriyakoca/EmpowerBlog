using System.ComponentModel.DataAnnotations;

namespace EmpowerBlog.Services.Review.API.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }

        [MaxLength(1000)]
        public string Text { get; set; }
    }
}
