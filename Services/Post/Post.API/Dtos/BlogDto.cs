using System.ComponentModel.DataAnnotations;

namespace EmpowerBlog.Services.Post.API.Dtos
{
    public class BlogDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
