using Microsoft.AspNetCore.Mvc;
using SANATIFY.Data;
using SANATIFY.Models;
using System.Data;
using System.Data.SqlClient;
using SANATIFY.Services;

namespace SANATIFY.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private int userId;
        private static string usreName;
        private string passWord;
        private readonly UserService _userService;

        public UserController(IConfiguration configuration, UserService userService)
        {
            _configuration = configuration;
            _userService = userService;
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

        [HttpGet]
        public IActionResult ArtistLogin()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string query = "SELECT COUNT(*) FROM Person WHERE UserName = @Username AND Password = @Password";
                var parameters = new[]
                {
                    new SqlParameter("@Username", model.Username),
                    new SqlParameter("@Password", model.Password)
                };
                passWord = model.Password;
                usreName = model.Username;
                DataTable result = _context.ExecuteQuery(query, parameters);

                if (result.Rows.Count > 0 && (int)result.Rows[0][0] > 0)
                {
                    userId = _userService.GetUserId(model.Username);
                    Console.WriteLine(usreName, passWord, userId);
                    int kindUser = _userService.GetUserKindId(model.Username, model.Password);
                    if (kindUser == 2)
                    {
                        return RedirectToAction("ArtistDashboard", "User");
                    }
                    else
                    {
                        return RedirectToAction("DisplayAllMusics", "Music");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ArtistLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string query =
                    "SELECT COUNT(*) FROM Person WHERE UserName = @Username AND Password = @Password AND Kind_ID = 2";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", model.Username),
                    new SqlParameter("@Password", model.Password)
                };

                DataTable result = _context.ExecuteQuery(query, parameters);

                if (result.Rows.Count > 0 && (int)result.Rows[0][0] > 0)
                {
                    userId = 4;
                    return RedirectToAction("ArtistDashboard", "User");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AddToLikeSong(int musicId)
        {
            int personId = userId;
            string query = "INSERT INTO Like_Music (Music_ID, Person_ID) VALUES (@Music_ID, @Person_ID)";
            var parameters = new[]
            {
                new SqlParameter("@Music_ID", musicId),
                new SqlParameter("@Person_ID", personId)
            };

            _context.ExecuteNonQuery(query, parameters);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Browse()
        {
            throw new NotImplementedException();
        }

        public IActionResult Library()
        {
            throw new NotImplementedException();
        }

        public IActionResult ArtistDashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddMusic()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddMusic(MusicViewModel model)
        {
            try
            {
                int personId = _userService.GetUserId(usreName);
                string query =
                    "INSERT INTO Music (Name, Genre_ID, Person_ID, Cover, Region, Ages, Date, Text, Playlist_Allow) " +
                    "VALUES (@Name, @Genre_ID, @Person_ID, @Cover, @Region, @Ages, @Date, @Text, @Playlist_Allow)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Name", model.Name),
                    new SqlParameter("@Person_ID", personId),
                    new SqlParameter("@Genre_ID", model.Genre_ID),
                    new SqlParameter("@Cover", model.Cover),
                    new SqlParameter("@Region", model.Region),
                    new SqlParameter("@Ages", model.Ages),
                    new SqlParameter("@Date", DateTime.Now),
                    new SqlParameter("@Text", model.Text),
                    new SqlParameter("@Playlist_Allow", model.Playlist_Allow)
                };

                _context.ExecuteNonQuery(query, parameters);
                return RedirectToAction("DisplayAllMusics", "Music");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                Console.WriteLine(ex.Message);
            }

            return View(model);
        }
    }
}