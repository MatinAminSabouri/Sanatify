using Microsoft.AspNetCore.Mvc;
using SANATIFY.Models;
using System.Data.SqlClient;

namespace SANATIFY.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
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
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Person (UserName, Password, Email, Country, State, City, Kind_ID, Wallet) " +
                                   "VALUES (@Username, @Password, @Email, @Country, @State, @City, @Kind, 0)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", model.Username);
                    command.Parameters.AddWithValue("@Password", model.Password);
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@Country", model.Country);
                    command.Parameters.AddWithValue("@State", model.State);
                    command.Parameters.AddWithValue("@City", model.City);
                    command.Parameters.AddWithValue("@Kind", model.Kind);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                return RedirectToAction("Index", "Home");
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
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Person WHERE UserName = @Username AND Password = @Password";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", model.Username);
                    command.Parameters.AddWithValue("@Password", model.Password);

                    connection.Open();
                    int userCount = (int)command.ExecuteScalar();
                    connection.Close();

                    if (userCount > 0)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Privacy", "Home");
                    }
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
