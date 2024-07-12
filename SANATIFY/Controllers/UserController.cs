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
                    int kindUser = _userService.GetUserKindId(model.Username, model.Password);
                    if (kindUser == 2)
                    {
                        return RedirectToAction("ArtistDashboard", "User");
                    }
                    else
                    {
                        return RedirectToAction("UserDashboard", "User");
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
            try
            {
                int personId = _userService.GetUserId(usreName);

                // Check if the song is already liked by the user
                string checkQuery =
                    "SELECT COUNT(*) FROM Like_Music WHERE Music_ID = @Music_ID AND Person_ID = @Person_ID";
                var checkParameters = new[]
                {
                    new SqlParameter("@Music_ID", musicId),
                    new SqlParameter("@Person_ID", personId)
                };
                DataTable checkResult = _context.ExecuteQuery(checkQuery, checkParameters);

                if (checkResult.Rows.Count > 0 && (int)checkResult.Rows[0][0] > 0)
                {
                    // Song is already liked by the user
                    return Json(new { success = false, message = "Song already liked." });
                }

                // Add the song to the user's likes
                string query = "INSERT INTO Like_Music (Music_ID, Person_ID) VALUES (@Music_ID, @Person_ID)";
                var parameters = new[]
                {
                    new SqlParameter("@Music_ID", musicId),
                    new SqlParameter("@Person_ID", personId)
                };

                _context.ExecuteNonQuery(query, parameters);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred." });
            }
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

        [HttpGet]
        public IActionResult AllArtistConcerts()
        {
            int personId = _userService.GetUserId(usreName);
            var concerts = GetConcertsByUserId(personId);
            return View(concerts);
        }

        private List<ConcertViewModel> GetConcertsByUserId(int userId)
        {
            string query = "SELECT ID, Date, Price, Valid FROM Concert WHERE Person_ID = @Person_ID";
            var parameters = new[] { new SqlParameter("@Person_ID", userId) };
            DataTable result = _context.ExecuteQuery(query, parameters);

            var concerts = new List<ConcertViewModel>();

            foreach (DataRow row in result.Rows)
            {
                concerts.Add(new ConcertViewModel
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Date = Convert.ToDateTime(row["Date"]),
                    Price = Convert.ToInt32(row["Price"]),
                    Valid = Convert.ToBoolean(row["Valid"])
                });
            }

            return concerts;
        }

        [HttpPost]
        public IActionResult CancelConcert(int concertId)
        {
            string query = "UPDATE Concert SET Valid = 0 WHERE ID = @ConcertId AND Person_ID = @Person_ID";
            var parameters = new[]
            {
                new SqlParameter("@ConcertId", concertId),
                new SqlParameter("@Person_ID", userId)
            };

            _context.ExecuteNonQuery(query, parameters);

            return RedirectToAction("AllArtistConcerts");
        }

        [HttpGet]
        public IActionResult AllConcerts()
        {
            string query = "SELECT ID, Date, Price, Valid FROM Concert WHERE Valid = 1";
            DataTable result = _context.ExecuteQuery(query, new SqlParameter[0]);


            List<ConcertViewModel> concerts = new List<ConcertViewModel>();

            decimal credit = _userService.GetUserCredit(usreName);
            ViewBag.UserCredit = credit;


            foreach (DataRow row in result.Rows)
            {
                concerts.Add(new ConcertViewModel
                {
                    ID = (int)row["ID"],
                    Date = (DateTime)row["Date"],
                    Price = (int)row["Price"],
                    Valid = (bool)row["Valid"]
                });
            }

            return View(concerts);
        }

        public IActionResult UserDashboard()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BuyTicket(int concertId)
        {
            string query = "SELECT Person_ID, Price, Valid FROM Concert WHERE ID = @ConcertId";
            var parameters = new[]
            {
                new SqlParameter("@ConcertId", concertId)
            };

            DataTable result = _context.ExecuteQuery(query, parameters);

            if (result.Rows.Count > 0)
            {
                int personId = _userService.GetUserId(usreName);
                int concertPrice = (int)result.Rows[0]["Price"];
                bool isValid = (bool)result.Rows[0]["Valid"];

                // Check if the user has enough credit and the concert is valid
                decimal userCredit = _userService.GetUserCredit(usreName);

                if (userCredit >= concertPrice && isValid)
                {
                    decimal newCredit = userCredit - concertPrice;
                    _userService.UpdateUserCredit(usreName, newCredit);


                    string insertQuery = "INSERT INTO Ticket (Person_ID, Concert_ID) VALUES (@Person_ID, @Concert_ID)";
                    SqlParameter[] insertParams = new SqlParameter[]
                    {
                        new SqlParameter("@Person_ID", personId),
                        new SqlParameter("@Concert_ID", concertId)
                    };

                    _context.ExecuteNonQuery(insertQuery, insertParams);

                    return RedirectToAction("AllConcerts");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Insufficient credit or concert is not valid.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Concert not found.");
            }

            return RedirectToAction("AllConcerts");
        }

        public IActionResult Browse() //Browse Users
        {
            var users = _userService.GetAllUsers();
            return View(users);
        }

        public IActionResult BrowseArtists()
        {
            var artists = _userService.GetAllArtists();
            return View(artists);
        }


        [HttpPost]
        public IActionResult FollowUser(int followingId)
        {
            int followerId = _userService.GetUserId(usreName);
            _userService.FollowUser(followerId, followingId);
            return RedirectToAction("Browse");
        }

        public IActionResult ViewArtistSongs(int artistId)
        {
            var songs = _userService.GetSongsByArtist(artistId);
            return View(songs);
        }

        public IActionResult MySentRequests()
        {
            int userId = _userService.GetUserId(usreName);
            var requests = _userService.GetSentFriendRequests(userId);
            return View(requests);
        }
        
        [HttpPost]
        public IActionResult SendFriendRequest(int receiverId)
        {
            try
            {
                int senderId = _userService.GetUserId(usreName);
                // Send the friend request
                _userService.SendFriendRequest(senderId, receiverId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult MyReceivedRequests()
        {
            try
            {
                int receiverId = _userService.GetUserId(usreName);
                var requests = _userService.GetReceivedFriendRequests(receiverId);
                return View(requests);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("UserDashboard");
            }
        }

        [HttpPost]
        public IActionResult AcceptFriendRequest(int requestId)
        {
            try
            {
                _userService.AcceptFriendRequest(requestId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult MyFriends()
        {
            try
            {
                int userId = _userService.GetUserId(usreName);
                var friends = _userService.GetFriends(userId);

                ViewBag.CurrentUserId = userId;


                return View(friends);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<UserViewModel>());
            }
        }

        [HttpPost]
        public IActionResult Unfriend(int friendId)
        {
            int userId = _userService.GetUserId(usreName);
            _userService.Unfriend(userId, friendId);
            return RedirectToAction("MyFriends");
        }

        public IActionResult ChatMessages(int friendId)
        {
            int userId = _userService.GetUserId(usreName);
            var messages = _userService.GetChatMessages(userId, friendId);

            List<MessageViewModel> messageViewModels = new List<MessageViewModel>();

            foreach (var msg in messages)
            {
                messageViewModels.Add(new MessageViewModel
                {
                    Date = msg.Date,
                    Text = msg.Text,
                    SenderName = usreName
                });
            }

            return Json(messageViewModels);
        }

        [HttpPost]
        public IActionResult SendMessage(int friendId, string message)
        {
            int userId = _userService.GetUserId(usreName);
            _userService.SendMessage(userId, friendId, message);
            return Ok();
        }

        public IActionResult ChatRoom(int friendId)
        {
            int userId = _userService.GetUserId(usreName);
            var messages = _userService.GetChatMessages(userId, friendId);

            List<MessageViewModel> messageViewModels = new List<MessageViewModel>();

            foreach (var msg in messages)
            {
                messageViewModels.Add(new MessageViewModel
                {
                    Date = msg.Date,
                    Text = msg.Text,
                    SenderName = msg.SenderName
                });
            }

            ViewBag.FriendId = friendId;
            ViewBag.CurrentUserId = userId;
            return View(messageViewModels);
        }

        [HttpPost]
        public IActionResult RejectFriendRequest(int requestId)
        {
            try
            {
                _userService.RejectFriendRequest(requestId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddComment(int musicId, string text)
        {
            try
            {
                int personId = _userService.GetUserId(usreName);
                _userService.AddComment(personId, musicId, text);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpGet("ShowComments")]
        public IActionResult ShowComments(int musicId)
        {
            try
            {
                var comments = _userService.GetCommentsForMusic(musicId);
                ViewBag.MusicId = musicId;
                return View(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}