using System.ComponentModel.DataAnnotations;

namespace lab11.Application.DTOs
{
    public class UpdateBookDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be from 2 to 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "AuthorId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be greater than 0")]
        public int AuthorId { get; set; }
    }
}