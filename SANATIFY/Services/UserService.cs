using System.Data;
using System.Data.SqlClient;
using SANATIFY.Data;

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

    }
}