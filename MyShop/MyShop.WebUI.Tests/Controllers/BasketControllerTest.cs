using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
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
    }
}
