using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using MyShop.Services;
using MyShop.WebUI.Controllers;
using MyShop.WebUI.Tests.Mocks;

namespace MyShop.WebUI.Tests.Controllers
{
    [TestClass]
    public class BasketControllerTest
    {
        [TestMethod]
        public void CanAddBasketItem()
        {
            IRepository<Product> productContext = new MockContext<Product>();
            //IRepository<ProductCategory> productCategoryContext = new MockContext<ProductCategory>();
            IRepository<Basket> basketContext = new MockContext<Basket>();

            IBasketService basketService = new BasketService(productContext,basketContext);

            //setting up
            BasketController basketController = new BasketController(basketService);
            basketController.ControllerContext = new System.Web.Mvc.ControllerContext(new MockHttpContext(),new System.Web.Routing.RouteData(),basketController);
            //basketService.AddToBasket(new MockHttpContext(),"1");
            
            //act
            basketController.AddToBasket("1");
            var basket = basketContext.Collection().FirstOrDefault();

            //Assert
            Assert.IsNotNull(basket);
            Assert.AreEqual(1, basket.BasketItems.Count());
            Assert.AreEqual("1",basket.BasketItems.ToList().FirstOrDefault().ProductId);

        }

        [TestMethod]
        public void CanGetSummaryViewModel() 
        {
            IRepository<Product> productContext = new MockContext<Product>();
            IRepository<Basket> basketContext = new MockContext<Basket>();
            IBasketService basketService = new BasketService(productContext, basketContext);

            productContext.Insert(new Product() { Id = "1", Price = 10.00m });
            productContext.Insert(new Product() { Id = "2", Price = 20.00m });

            Basket basket = new Basket();

            basket.BasketItems.Add(new BasketItem() {ProductId="1", Quantity=2 });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 3 });
            basketContext.Insert(basket);

            var basketController = new BasketController(basketService);

            MockHttpContext httpContext = new MockHttpContext();
            httpContext.Request.Cookies.Add(new System.Web.HttpCookie("eCommerceBasket") {Value = basket.Id });
            basketController.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext, new System.Web.Routing.RouteData(), basketController);

            var result= basketController.BasketSummary() as PartialViewResult;
            var basketSummary= (BasketSummaryViewModel)result.ViewData.Model;

            Assert.AreEqual(5, basketSummary.BasketCount);
            Assert.AreEqual(80.00m, basketSummary.BasketTotal);

        }

    }
}
