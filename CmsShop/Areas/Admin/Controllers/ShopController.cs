using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using PagedList;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            // zadeklarowanie listy kategorii do wyswietlenia 
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                                   .ToArray()
                                   .OrderBy(x => x.Sorting)
                                   .Select(x => new CategoryVM(x)).ToList();
            }

                return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // Deklaracja ID
            string id;

            using (Db db = new Db())
            {
                // Sprawdzenie czy dana nazwa kategorii jest unikalna
                if (db.Categories.Any(x => x.Name == catName))
                    return "tytulzajety";

                // Inicjalizacja DTO
                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;

                // Zapis do bazy 
                db.Categories.Add(dto);
                db.SaveChanges();

                // Pobieranie ID
                id = dto.Id.ToString();
            }

            return id;
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories (int[] id)
        {
            using (Db db = new Db())
            {
                // inicjalizacja licznika do sortowania kategorii
                int count = 1;

                // deklaracj DTO 
                CategoryDTO dto;

                // sortowanie kategorii
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    // Zapis na bazie
                    db.SaveChanges();

                    count++;
                }
            }

                return View();
        }

        // GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                // Pobieranie kateorii o podanym id
                CategoryDTO dto = db.Categories.Find(id);

                // Usuwanie Kategorii
                db.Categories.Remove(dto);

                // Zapis do bazy
                db.SaveChanges();

            }

                return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCateory
        [HttpPost]
        public string RenameCateory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Sprawdzania czy kategoria jest unikalna
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "tytulzajety";

                // Pobieramy kategorii
                CategoryDTO dto = db.Categories.Find(id);

                // Edycja kategorii
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // Zapis na bazie
                db.SaveChanges();


            }

                return "OK";
        }

        // GET: Admin/Shop/Categories
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Inicjalizacja model
            ProductVM model = new ProductVM();

            // Pobieranie listy kategorii
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

                return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // sprawdzamy model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // sprawdzenie czy nazwa produktu jest unikalna
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta!");
                    return View(model);
                }
            }

            // deklaracja product id
            int id;

            // dodawanie produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // pobranie id dodanego produktu
                id = product.Id;
            }

            // ustawiamy komunikat 
            TempData["SM"] = "Dodałeś produkt";

            #region Upload Image

            // Utworzenie potrzebnej struktury katalogów
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);


            if (file != null && file.ContentLength > 0)
            {
                // sprawdzenie rozszezenia pliku czy mamy do czynienia z obrazkiem
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }
                }

                // inicjalizacja nazwy obrazka
                string imageName = file.FileName;

                // zapis nazwy obrazka do bazy
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                // zapisujemy orginalny obrazek
                file.SaveAs(path);

                // zapisujemy miniaturke
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }


            #endregion

            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/AddProducts
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // Deklaracja listy produktów
            List<ProductVM> listOfProductVM;

            // Ustawiamy numer strony
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // inicjalizacja listy produktu
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                // lista kateori w sekwencji dropDownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // ustawienie wybranych katerorii
                ViewBag.SelectedCat = catId.ToString();
                
            }

            // ustawianie stronicowania
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            // zwracanie widoku produktu z listy
            return View(listOfProductVM);
        }
    }
}