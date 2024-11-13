using System.ComponentModel.DataAnnotations;

namespace Calendary.Api.Dtos
{
    public class EventDateDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }


        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
