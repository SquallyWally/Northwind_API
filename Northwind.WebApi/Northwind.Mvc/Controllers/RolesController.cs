using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Northwind.Mvc.Controllers;

public class RolesController : Controller
{
    private const string AdminRole = "Administrators";
    private const string UserEmail = "testt@example.com";

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    
    private readonly ILogger<HomeController> _logger;
    

    public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ILogger<HomeController> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        // Checks of AdminRole exists, else create with role manager
        if (!(await _roleManager.RoleExistsAsync(AdminRole)))
        {
            await _roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        //create Identity user based by mail
        IdentityUser user = await _userManager.FindByEmailAsync(UserEmail);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = UserEmail,
                Email = UserEmail
            };
            IdentityResult result = await _userManager.CreateAsync(user, "W@chtw00rd");
            //error handling
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} created successfully", user.UserName);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }
        
        // crete usermanager token
        if (!user.EmailConfirmed)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            //Search for user by its email
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} email confirmed successfully", user.UserName);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }
        
        //Assign user to AdminRole
        if (!(await _userManager.IsInRoleAsync(user, AdminRole)))
        {
            IdentityResult result = await _userManager.AddToRoleAsync(user, AdminRole);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} added to {AdminRole} successfully", user.UserName, AdminRole);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }
        return Redirect("/");
    }
}