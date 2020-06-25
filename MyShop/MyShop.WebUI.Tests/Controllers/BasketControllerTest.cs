using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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
            IRepository<Customer> customerRepository = new MockContext<Customer>();


            IBasketService basketService = new BasketService(productContext,basketContext);
            IOrderService orderService = new OrderService(new MockContext<Order>());
            //setting up
            BasketController basketController = new BasketController(basketService,orderService,customerRepository);
           
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
            IRepository<Customer> customerRepository = new MockContext<Customer>();
            IBasketService basketService = new BasketService(productContext, basketContext);
            IOrderService orderService = new OrderService(new MockContext<Order>());

            productContext.Insert(new Product() { Id = "1", Price = 10.00m });
            productContext.Insert(new Product() { Id = "2", Price = 20.00m });

            Basket basket = new Basket();

            basket.BasketItems.Add(new BasketItem() {ProductId="1", Quantity=2 });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 3 });
            basketContext.Insert(basket);

            var basketController = new BasketController(basketService,orderService,customerRepository);

            MockHttpContext httpContext = new MockHttpContext();
            httpContext.Request.Cookies.Add(new System.Web.HttpCookie("eCommerceBasket") {Value = basket.Id });
            basketController.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext, new System.Web.Routing.RouteData(), basketController);

            var result= basketController.BasketSummary() as PartialViewResult;
            var basketSummary= (BasketSummaryViewModel)result.ViewData.Model;

            Assert.AreEqual(5, basketSummary.BasketCount);
            Assert.AreEqual(80.00m, basketSummary.BasketTotal);

        }

        [TestMethod]
        public void CanCheckoutAndCreateOrder() 
        {
            IRepository<Product> productContext = new MockContext<Product>();
            IRepository<Basket> basketContext = new MockContext<Basket>();
            IRepository<Order> orderContext = new MockContext<Order>();
            IRepository<Customer> customerRepository = new MockContext<Customer>();

            IBasketService basketService = new BasketService(productContext, basketContext);
            IOrderService orderService = new OrderService(orderContext);


            productContext.Insert(new Product() { Id = "1", Price = 10.00m });
            productContext.Insert(new Product() { Id = "2", Price = 20.00m });

            customerRepository.Insert(new Customer { Id = "1", Email = "email@email.com", ZipCode = "00001", });
            IPrincipal FakeUser = new GenericPrincipal(new GenericIdentity("email@email.com", type: "Forms"), null);

            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 2, BasketId=basket.Id });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 3, BasketId = basket.Id });
            basketContext.Insert(basket);

            var controller = new BasketController(basketService,orderService,customerRepository);
            var httpContext = new MockHttpContext();
            httpContext.User = FakeUser;
            httpContext.Request.Cookies.Add(new System.Web.HttpCookie("eCommerceBasket") { Value = basket.Id });

            controller.ControllerContext = new ControllerContext(httpContext, new System.Web.Routing.RouteData(),controller);

            //Act
            Order order = new Order();
            controller.Checkout(order);

            //Asserts
            Assert.AreEqual(2, order.OrderItems.Count);
            Assert.AreEqual(0,basket.BasketItems.Count);

            Order orderInRep = orderContext.Find(order.Id);
            Assert.IsNotNull(orderInRep);
            Assert.AreEqual(2,orderInRep.OrderItems.Count);
        }

    }
}
