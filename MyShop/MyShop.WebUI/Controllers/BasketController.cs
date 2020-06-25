using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyShop.WebUI.Controllers
{
    public class BasketController : Controller
    {
        IBasketService basketService;
        IOrderService orderService;
        IRepository<Customer> customerRepository;

        public BasketController(IBasketService basketService,IOrderService orderService, IRepository<Customer> customerRepository)
        {
            this.basketService = basketService;
            this.orderService = orderService;
            this.customerRepository = customerRepository;
        }
        // GET: Basket
        public ActionResult Index()
        {
            var model = basketService.GetBasketItems(this.HttpContext);
            return View(model);
        }

        public ActionResult AddToBasket(string Id)
        {
            basketService.AddToBasket(this.HttpContext, Id);
            return RedirectToAction("Index");
        }
        public ActionResult RemoveFromBasket(string Id)
        {
            basketService.RemoveFromBasket(this.HttpContext, Id);
            return RedirectToAction("Index");
        }

        public PartialViewResult BasketSummary()
        {
            var basketSummary = basketService.GetBasketSummary(this.HttpContext);
            return PartialView(basketSummary);
        }

        [Authorize]
        public ActionResult Checkout() 
        {
            Customer customer = customerRepository.Collection().FirstOrDefault(c=>c.Email==User.Identity.Name);

            if (customer!=null)
            {
                Order order = new Order 
                {
                    Email = customer.Email,
                    City = customer.City,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    State = customer.State,
                    Street = customer.Street,
                    ZipCode = customer.ZipCode
                };
                return View(order);

            }
            else
            {
                return RedirectToAction("Error");
            }

           
        }


        [HttpPost]
        [Authorize]
        public ActionResult Checkout(Order order)
        {
            var basketItems = basketService.GetBasketItems(this.HttpContext);
            order.OrderStatus = "Order Created";
            order.Email = User.Identity.Name;
            //Process payment


            order.OrderStatus = "Payment Process";
            orderService.CreateOrder(order,basketItems.ToList());
            basketService.ClearBasket(this.HttpContext);
            return RedirectToAction("Thankyou",new { OrderId=order.Id });
        }

        public ActionResult Thankyou(string OrderId) 
        {
            ViewBag.OrderId = OrderId;
            return View();
        }

    }
}