using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
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

    }
}