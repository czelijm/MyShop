using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{
    public class BasketService : IBasketService
    {
        IRepository<Product> productContext;
        IRepository<Basket> basketContext;
        public const string BasketSessionName = "eCommerceBasket";

        public BasketService(IRepository<Product> ProductContext, IRepository<Basket> BasketContext)
        {
            this.productContext = ProductContext;
            this.basketContext = BasketContext;
        }

        private Basket GetBasket(HttpContextBase httpContextBase, bool CreateIfNull)
        {
            HttpCookie cookie = httpContextBase.Request.Cookies.Get(BasketSessionName);

            Basket basket = new Basket();

            if (cookie != null)
            {
                string basketId = cookie.Value;
                if (!string.IsNullOrEmpty(basketId))
                {
                    basket = basketContext.Find(basketId);
                }
                else
                {
                    if (CreateIfNull)
                    {
                        basket = CreateNewBasket(httpContextBase);
                    }
                }
            }
            else
            {
                if (CreateIfNull)
                {
                    basket = CreateNewBasket(httpContextBase);
                }
            }

            return basket;
        }

        private Basket CreateNewBasket(HttpContextBase httpContextBase)
        {
            Basket basket = new Basket();
            basketContext.Insert(basket);
            basketContext.Commit();

            HttpCookie cookie = new HttpCookie(BasketSessionName);
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);
            httpContextBase.Response.Cookies.Add(cookie);
            return basket;
        }

        public void AddToBasket(HttpContextBase httpContextBase, string productId)
        {
            Basket basket = GetBasket(httpContextBase, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                item = new BasketItem()
                {
                    BasketId = basket.Id,
                    ProductId = productId,
                    Quantity = 1
                };
                basket.BasketItems.Add(item);
            }
            else
            {
                item.Quantity++;
            }
            basketContext.Commit();
        }

        public void RemoveFromBasket(HttpContextBase httpContextBase, string Id)
        {
            Basket basket = GetBasket(httpContextBase, true);
            //Basket basket = basketContext.Find();
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.Id == Id);
            if (item != null)
            {
                basket.BasketItems.Remove(item);
                basketContext.Commit();
            }
        }

        public IEnumerable<BasketItemViewModel> GetBasketItems(HttpContextBase httpContextBase)
        {
            Basket basket = GetBasket(httpContextBase, false);

            if (basket != null)
            {
                var results = (from b in basket.BasketItems
                               join p in productContext.Collection() on b.ProductId equals p.Id
                               select new BasketItemViewModel()
                               {
                                   Id = b.Id,
                                   Image = p.Image,
                                   Price = p.Price,
                                   ProductName = p.Name,
                                   Quantity = b.Quantity

                               }).ToList();
                return results;
            }
            else
            {
                return new List<BasketItemViewModel>();
            }
        }

        public BasketSummaryViewModel GetBasketSummary(HttpContextBase httpContextBase)
        {
            Basket basket = GetBasket(httpContextBase, false);
            BasketSummaryViewModel model = new BasketSummaryViewModel(0, 0);
            if (basket != null)
            {
                //allow null value
                int? basketCount = (from item in basket.BasketItems
                                    select item.Quantity
                                    ).Sum();

                decimal? basketTotal = (from item in basket.BasketItems
                                        join p in productContext.Collection()
                                        on item.ProductId equals p.Id
                                        select item.Quantity * p.Price).Sum();

                model.BasketCount = basketCount ?? 0;
                model.BasketTotal = basketTotal ?? decimal.Zero;
                return model;

                //var result = (from b in basket.BasketItems
                //              join p in productContext.Collection()
                //              on b.Id equals p.Id
                //              select new BasketSummaryViewModel( ));
            }
            else
            {
                return model;
            }

        }
    }
}
