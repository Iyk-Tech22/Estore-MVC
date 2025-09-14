

namespace Estore.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCartModel> Carts { get; set; }
        public OrderHeaderModel OrderHeader { get; set; }
    }
}
