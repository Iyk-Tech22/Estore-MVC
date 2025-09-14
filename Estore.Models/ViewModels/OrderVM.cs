using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estore.Models.ViewModels
{
    public class OrderVM
    {
        public OrderHeaderModel OrderHeader { get; set; }
        public IEnumerable<OrderDetailModel> OrderDetailList { get; set; }
    }
}
