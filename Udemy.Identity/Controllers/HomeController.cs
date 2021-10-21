using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Udemy.Identity.Entities;
using Udemy.Identity.Models;

namespace Udemy.Identity.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;

        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Create()
        {
            return View(new UserCreateModel());
        }
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserSignInModel model)
        {
            if (ModelState.IsValid)
            {
                var siginResult = await _signInManager.PasswordSignInAsync(model.Username, model.Password, true, false);
                if (siginResult.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                        return RedirectToAction("AdminPanel");

                    return RedirectToAction("Panel");

                }
                else if (siginResult.IsLockedOut)
                {
                    //hesap kilitli
                }
                else if (siginResult.IsNotAllowed)
                {
                    //doğrulama 
                }

            }

            return View(model);
        }

        [Authorize]
        public IActionResult GetUserInfo()
        {
            var username = User.Identity.Name;
            var role = User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Role);
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel()
        {

            return View();
        }
        [Authorize(Roles = "Member")]
        public IActionResult Panel()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new()
                {
                    Email = model.Email,
                    Gender = model.Gender,
                    UserName = model.username
                };

                var identityResult = await _userManager.CreateAsync(user, model.Password);
                if (identityResult.Succeeded)
                {
                    await _roleManager.CreateAsync(new AppRole
                    {
                        Name = "Member",
                        CreatedTime = DateTime.Now
                    });

                    await _userManager.AddToRoleAsync(user, "Member");

                    return RedirectToAction("Index");
                }

                foreach (var item in identityResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

            }
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
           await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}
