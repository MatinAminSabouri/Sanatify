using System.Data;
using System.Data.SqlClient;
using SANATIFY.Data;
using SANATIFY.Models;

namespace SANATIFY.Services
{
    public class UserService
    {
        private readonly AppDbContext _appDbContext;

        public UserService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public void RegisterUser(string username, string passwordHash, string email, string country, string state,
            string city, int kindId)
        {
            string query = "INSERT INTO Person (UserName, Password, Email, Country, State, City, Kind_ID) " +
                           "VALUES (@Username, @Password, @Email, @Country, @State, @City, @Kind_ID)";
            var parameters = new[]
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", passwordHash),
                new SqlParameter("@Email", email),
                new SqlParameter("@Country", country),
                new SqlParameter("@State", state),
                new SqlParameter("@City", city),
                new SqlParameter("@Kind_ID", kindId)
            };
            _appDbContext.ExecuteNonQuery(query, parameters);
        }

        public DataTable GetUser(string username)
        {
            var parameters = new[] { new SqlParameter("@Username", username) };
            string query = "SELECT * FROM Person WHERE UserName = @Username";
            return _appDbContext.ExecuteQuery(query, parameters);
        }

        public int GetUserKindId(string username, string password)
        {
            string query = "SELECT Kind_ID FROM Person WHERE UserName = @Username AND Password = @Password";
            var parameters = new[]
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password)
            };

            var result = _appDbContext.ExecuteQuery(query, parameters);

            if (result.Rows.Count == 1)
            {
                return (int)result.Rows[0]["Kind_ID"];
            }

            return -1; 
        }
        public int GetUserId(string username)
        {
            string query = "SELECT ID FROM Person WHERE UserName = @Username";
            var parameters = new[]
            {
                new SqlParameter("@Username", username)
            };
            DataTable result = _appDbContext.ExecuteQuery(query, parameters);

            if (result.Rows.Count > 0)
            {
                return Convert.ToInt32(result.Rows[0]["ID"]);
            }
            else
            {
                throw new Exception("User not found.");
            }
        }
        public int GetUserCredit(string username)
        {
            string query = "SELECT Wallet FROM Person WHERE UserName = @Username";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };

            DataTable result = _appDbContext.ExecuteQuery(query, parameters);

            if (result.Rows.Count > 0)
            {
                return (int)result.Rows[0]["Wallet"];
            }

            throw new Exception("User not found.");
        }
        public void UpdateUserCredit(string username, decimal newCredit)
        {
            string query = "UPDATE Person SET Wallet = @NewCredit WHERE UserName = @Username";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@NewCredit", newCredit),
                new SqlParameter("@Username", username)
            };

            _appDbContext.ExecuteNonQuery(query, parameters);
        }
        public void SendFriendRequest(int senderId, int receiverId)
        {
            string query = "INSERT INTO Freind_Req (Person_Sender_ID, Person_Rec_ID, Accept, State, Date) " +
                           "VALUES (@SenderId, @ReceiverId, 0, 1, @Date)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@SenderId", senderId),
                new SqlParameter("@ReceiverId", receiverId),
                new SqlParameter("@Date", DateTime.Now)
            };

            _appDbContext.ExecuteNonQuery(query, parameters);
        }

        public void FollowUser(int followerId, int followingId)
        {
            string query = "INSERT INTO Following (Person_Follower_ID, Person_Following_ID) " +
                           "VALUES (@FollowerId, @FollowingId)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@FollowerId", followerId),
                new SqlParameter("@FollowingId", followingId)
            };

            _appDbContext.ExecuteNonQuery(query, parameters);
        }
        public List<UserViewModel> GetAllUsers()
        {
            string query = "SELECT ID, UserName, Email FROM Person";
            DataTable result = _appDbContext.ExecuteQuery(query, new SqlParameter[0]);

            List<UserViewModel> users = new List<UserViewModel>();

            foreach (DataRow row in result.Rows)
            {
                users.Add(new UserViewModel
                {
                    ID = (int)row["ID"],
                    UserName = row["UserName"].ToString(),
                    Email = row["Email"].ToString()
                });
            }

            return users;
        }
        public List<UserViewModel> GetAllArtists()
        {
            string query = "SELECT ID, UserName, Email FROM Person WHERE Kind_ID = 2";
            DataTable result = _appDbContext.ExecuteQuery(query, new SqlParameter[0]);

            List<UserViewModel> artists = new List<UserViewModel>();

            foreach (DataRow row in result.Rows)
            {
                artists.Add(new UserViewModel
                {
                    ID = (int)row["ID"],
                    UserName = row["UserName"].ToString(),
                    Email = row["Email"].ToString()
                });
            }

            return artists;
        }

        public List<MusicViewModel> GetSongsByArtist(int artistId)
        {
            string query = "SELECT ID, Name, Genre_ID, Cover FROM Music WHERE Person_ID = @ArtistId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ArtistId", artistId)
            };
            DataTable result = _appDbContext.ExecuteQuery(query, parameters);

            List<MusicViewModel> songs = new List<MusicViewModel>();

            foreach (DataRow row in result.Rows)
            {
                songs.Add(new MusicViewModel
                {
                    ID = (int)row["ID"],
                    Name = row["Name"].ToString(),
                    Genre_ID = (int)row["Genre_ID"],
                    Cover = (int)row["Cover"]
                });
            }

            return songs;
        }
    }
}