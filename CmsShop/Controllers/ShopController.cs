using CmsShop.Areas.Admin.Models.ViewModels.Shop;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }


        public ActionResult CategoryMenuPartial()
        {
            // deklarujemy CategoryVM list
            List<CategoryVM> categoryVMList;

            // inicjalizacja listy
            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                                   .ToArray()
                                   .OrderBy(x => x.Sorting)
                                   .Select(x => new CategoryVM(x))
                                   .ToList();
            }

            // zwracamy partial z lista
            return PartialView(categoryVMList);
        }

        // GET: /shop/category/name
        public ActionResult Category(string name)
        {
            // deklaracja ProduktVMList
            List<ProductVM> produktVMList;

            using (Db db = new Db())
            {
                // pobieranie id kategorii
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                // inicjalizacja listy produktów
                produktVMList = db.Products
                                  .ToArray()
                                  .Where(x => x.CategoryId == catId)
                                  .Select(x => new ProductVM(x)).ToList();

                // pobieramy nazwe kategorii
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;


            }

            // zwracamy widok z listy produktów z danej kategorii


            return View(produktVMList);
        }

        // GET: /shop/product-szczegoly/name
        [ActionName("product-szczegoly")]
        public ActionResult ProductDetails(string name)
        {
            // deklaracja productVM i productDTO
            ProductVM model;
            ProductDTO dto;

            // Inicjalizacja product id
            int id = 0;

            using (Db db = new Db())
            {
                // sprawdzamy czy produkt istnieje
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // inicjalizacja productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // pobranie id
                id = dto.Id;

                // inicjalizacja modelu
                model = new ProductVM(dto);
            }

            // pobieramy galerie zdjec dla wybranego produktu
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/uploads/Products/" + id + "/Gallery/Thumbs"))
                                            .Select(fn => Path.GetFileName(fn));

            // zwracamy widok z modelem
            return View("ProductDetails", model);
        }


        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            // inicjalizacja OrderForAdminVM
            List<OrdersForAdminVM> ordersForAdminVM = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // pobieramy zamowienia
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    // inicjalizacja slownika produktow
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    // inicjalizacja oredersDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // pobieramy uzytkownika 
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.UserName;

                    foreach( var orderDetails in orderDetailsList)
                    {
                        // pobieramy produkt
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // pobieramy cene produktu
                        decimal price = product.Price;

                        // pobieramy nazwe produktu
                        string productName = product.Name;

                        // dodac produkt do slownika
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // ustawiamy wartosc
                        total += orderDetails.Quantity * price;

                    }

                    ordersForAdminVM.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });

                }
            }

                return View(ordersForAdminVM);
        }

    }
}