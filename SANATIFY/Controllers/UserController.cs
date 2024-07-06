using Microsoft.AspNetCore.Mvc;
using SANATIFY.Data;
using SANATIFY.Models;
using System.Data;
using System.Data.SqlClient;

namespace SANATIFY.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            _context = new AppDbContext(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                string query =
                    "INSERT INTO Person (UserName, Password, Email, Country, State, City, Kind_ID, Wallet) " +
                    "VALUES (@Username, @Password, @Email, @Country, @State, @City, @Kind, 100)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", model.Username),
                    new SqlParameter("@Password", model.Password),
                    new SqlParameter("@Email", model.Email),
                    new SqlParameter("@Country", model.Country),
                    new SqlParameter("@State", model.State),
                    new SqlParameter("@City", model.City),
                    new SqlParameter("@Kind", model.Kind)
                };

                _context.ExecuteNonQuery(query, parameters);

                return RedirectToAction("DisplayAllMusics", "Music");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string query = "SELECT COUNT(*) FROM Person WHERE UserName = @Username AND Password = @Password";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", model.Username),
                    new SqlParameter("@Password", model.Password)
                };

                DataTable result = _context.ExecuteQuery(query, parameters);

                if (result.Rows.Count > 0 && (int)result.Rows[0][0] > 0)
                {
                    return RedirectToAction("DisplayAllMusics", "Music");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        public IActionResult Browse()
        {
            throw new NotImplementedException();
        }

        public IActionResult Library()
        {
            throw new NotImplementedException();
        }

        public IActionResult LikedSongs()
        {
            throw new NotImplementedException();
        }
    }
}