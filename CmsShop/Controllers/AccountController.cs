﻿using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShop.Controllers
{
    public class AccountController : Controller
    {

        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/login
        [HttpGet]
        public ActionResult Login()
        {
            //  sprawdzenie logowania użytkownika (czy nie jest zalogowany)
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");

            // zwracamy widok
            return View();
        }

        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzamy uzytkownika
            bool isValid = false;
            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika lub hasło");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
        }

        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }

        // POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // sprawdzenie hasła
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Hasła nie pasują do siebie");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // sprawdzenie czy nazwa uzytkownika jest unikalna
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika " + model.UserName + " jest już zajęta");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                // utworzenie uzytkownika
                UserDTO usserDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password,
                };

                // dodanie uzytkownika
                db.Users.Add(usserDTO);
                // zapis na bazie
                db.SaveChanges();

                // dodanie roli dla uzytkownika
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = usserDTO.Id,
                    RoleId = 2
                };

                // dodanie roli
                db.UserRoles.Add(userRoleDTO);
                // zapis na bazie
                db.SaveChanges();

            }

            // TempData komunikat 
            TempData["SM"] = "Jesteś zarejestrowany - możesz się zalogować";


            return Redirect("~/account/login");
        }

        // GET: /account/logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();


            return Redirect("~/account/login");
        }


        public ActionResult UserNavPartial()
        {
            // pobieramy username
            string username = User.Identity.Name;

            // deklaracja model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // pobieramy użytkownika 
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            return PartialView(model);
        }

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            // pobieramy nazwe użytkownika 
            string username = User.Identity.Name;

            // deklarujemy model 
            UserProfileVM model;

            using (Db db = new Db())
            {
                // pobieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserProfileVM(dto);
            }

                return View("UserProfile", model);
        }
    }
}