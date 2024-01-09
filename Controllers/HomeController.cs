using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers;

public class SessionCheckAttribute : ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        if (userId == null) {
            context.Result = new RedirectToActionResult("Auth", "Home", null);
        }
    }
}

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    [SessionCheck]
    public IActionResult Index()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        
        User currentUser = _context.Users.FirstOrDefault(u => u.UserId == userId);

        ViewBag.UserName = $"{currentUser.Name} {currentUser.LastName}";

        ViewBag.weddingTable = _context.Weddings.Include(e => e.Invitations).OrderBy(e => e.WeddingDate).ToList();

        return View();
    }

    [HttpGet("Auth")]
    public IActionResult Auth() {
        return View();
    }

    [HttpPost("Register")]
    public IActionResult Register(User userFromForm) {
        if (ModelState.IsValid) {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            userFromForm.Password = Hasher.HashPassword(userFromForm, userFromForm.Password);
            _context.Add(userFromForm);
            _context.SaveChanges();

            return RedirectToAction("Auth");   
        }
        return View("Auth");
    }

    [HttpPost]
    public IActionResult Login(LoginUser registeredUser) {
        if (ModelState.IsValid) {
            User userFromDb = _context.Users.FirstOrDefault(e => e.Email == registeredUser.LoginEmail);

            if (userFromDb == null) {
                ModelState.AddModelError("LoginEmail", "Invalid email address.");
                return View("Auth");
            }

            PasswordHasher<LoginUser> Hasher = new PasswordHasher<LoginUser>();
            
            var result = Hasher.VerifyHashedPassword(registeredUser, userFromDb.Password, registeredUser.LoginPassword);

            if (result == 0) {
                ModelState.AddModelError("LoginPassword", "Invalid password.");
                return View("Auth");
            }

            HttpContext.Session.SetInt32("UserId", userFromDb.UserId);
            return RedirectToAction("Index");
        }

        return View("Auth");
    }

    [HttpGet("Logout")]
    public IActionResult Logout() {
        HttpContext.Session.Clear();
        return RedirectToAction("Auth");
    }

    [SessionCheck]
    [HttpGet("weddings/new")]
    public IActionResult PlanAWedding()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
        ViewBag.UserName = $"{currentUser.Name} {currentUser.LastName}";

        return View();
    }

    [SessionCheck]
    [HttpPost("CreateWedding")]
    public IActionResult CreateWedding(Wedding weddingFromForm) {
        int? userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
        ViewBag.UserName = $"{currentUser.Name} {currentUser.LastName}";
        if (ModelState.IsValid) {
                weddingFromForm.UserId = userId;

                _context.Add(weddingFromForm);
                _context.SaveChanges();

                return RedirectToAction("WeddingDetails", new { id = weddingFromForm.WeddingId });
        }
        return View("PlanAWedding");
    }

    [SessionCheck]
    [HttpGet("weddings/{id}")]
    public IActionResult WeddingDetails(int id) {
        int? userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
        ViewBag.UserName = $"{currentUser.Name} {currentUser.LastName}";

        Wedding weddingDetails = _context.Weddings
        .Include(w => w.Invitations)
        .ThenInclude(i => i.Invited)
        .FirstOrDefault(w => w.WeddingId == id);

        return View(weddingDetails);
    }

    [SessionCheck]
    [HttpGet("AttendWedding")]
    public IActionResult AttendWedding(int id){
        int? userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
        ViewBag.UserName = $"{currentUser.Name} {currentUser.LastName}";
        
        Invitation attendeeFromDb = new Invitation();
        attendeeFromDb.WeddingId = id;
        attendeeFromDb.UserId = HttpContext.Session.GetInt32("UserId");
        _context.Add(attendeeFromDb);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [SessionCheck]
    [HttpGet("DoNotAttendWedding")]
    public IActionResult DoNotAttendWedding(int id){
        int? UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = UserId;
        User currentUser = _context.Users.FirstOrDefault(u => u.UserId == UserId);
        ViewBag.UserName = $"{currentUser.Name} {currentUser.LastName}";
        
        Invitation attendeeFromDb = _context.Invitations.FirstOrDefault(e=> e.WeddingId == id && e.UserId == UserId);
        _context.Remove(attendeeFromDb);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [SessionCheck]
    [HttpGet("DeleteWedding")]
    public IActionResult DeleteWedding(int id){
        int? userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.userId = userId;
        User currentUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
        ViewBag.UserName = $"{currentUser.Name} {currentUser.LastName}";

        Wedding deleteWedding = _context.Weddings.Include(e => e.Invitations).FirstOrDefault(e=> e.WeddingId == id);
        _context.Remove(deleteWedding);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}