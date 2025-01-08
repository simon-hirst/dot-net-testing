using System.ComponentModel.DataAnnotations;

namespace CrudDotNetTesting.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}