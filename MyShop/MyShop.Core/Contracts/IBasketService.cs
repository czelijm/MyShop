using MyShop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Core.Contracts
{
    public interface IBasketService
    {
        void AddToBasket(HttpContextBase httpContextBase, string productId);
        IEnumerable<BasketItemViewModel> GetBasketItems(HttpContextBase httpContextBase);
        BasketSummaryViewModel GetBasketSummary(HttpContextBase httpContextBase);
        void RemoveFromBasket(HttpContextBase httpContextBase, string itemId);
        void ClearBasket(HttpContextBase httpContextBase);
    }
}
