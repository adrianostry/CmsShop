using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            // zadeklarowanie listy (PageVM)
            List<PageVM> pagesList;

            using (Db db = new Db())
            {
                // zainicjalizowanie listy
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            // zwracanie strony do widoku

            return View(pagesList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {

            // Sprawdzanie stanu formularza 
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {

                string slug;

                // Inicjalizacja naszego PageDTO
                PageDTO dto = new PageDTO();

                dto.Title = model.Title;

                // Brak adresu strony to przypisujemy tytuł
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                // funkcja która pomaga aby nie dodawać takiej samej strony
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Ten tytuł lub adres strony już istnieje.");
                    return View(model);
                }

                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 1000;

                // Zapis DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "Dodałeś nową stronę";

            return RedirectToAction("AddPage");
        }

        // GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // deklaracja PageVM
            PageVM model;

            using (Db db = new Db())
            {
                // pobieranie z bazy strony o przekazanym id
                PageDTO dto = db.Pages.Find(id);

                // sprawdzanie czy strona istnieje 
                if (dto == null)
                {
                    return Content("Strona nie istnieje");
                }

                model = new PageVM(dto);
            }


                return View(model);
        }

        // POST: Admin/Pages/EditPage
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // pobranie id strony którą chcemy edytować
                int id = model.Id;

                // zainicjowanie slug
                string slug = "home";

                // Pobranie strony do edycji
                PageDTO dto = db.Pages.Find(id);

                if (model.Slug !="home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                // sprawdzamy czy strona/adres jest unikalny
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Strona lub adres strony już istnieje");
                }

                // modyfikacje DTO
                dto.Title = model.Title;
                dto.Slug = slug;
                dto.HasSidebar = model.HasSidebar;
                dto.Body = model.Body;
                // zapis do bazy edytowanej strony
                db.SaveChanges();
            }

            // ustawianie komunikatu
            TempData["SM"] = "Wyedytowałeś stronę";


            // Redirect
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/Details/id
        public ActionResult Details(int id)
        {
            // deklaracja PageVM
            PageVM model;

            using (Db db = new Db())
            {
                // Pobranie strony o id
                PageDTO dto = db.Pages.Find(id);

                // Sprawdzenie istnienia strony o takim id
                if (dto == null)
                {
                    return Content("Strona o podanym id nie istnieje.");
                }

                // inicjalizacja PagesVM
                model = new PageVM(dto);

            }

                return View(model);
        }

        // GET: Admin/Pages/Deleted/id
        public ActionResult Delete(int id)
        {
            using (Db db = new Db())
            {
                // Pobranie strony do usuniecia
                PageDTO dto = db.Pages.Find(id);

                // Usuwanie z bazy wybranej strony
                db.Pages.Remove(dto);

                // Zapisanie wszelkich zmian
                db.SaveChanges();
            }

            // Dodać komunikat Redirect - przekierowanie
            return RedirectToAction("Index");
        }
    }
}