﻿using CmsShop.Models.Data;
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
    }
}