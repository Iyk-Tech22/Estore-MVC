using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Estore.Models
{
    public class ShoppingCartModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Range(1, 1000, ErrorMessage = "Please enter a valid qty between 1 to 1000")]
        public int Quantity { get; set; }

        [NotMapped]
        public double Price { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public ProductModel Product { get; set; }
        
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

    }
}
