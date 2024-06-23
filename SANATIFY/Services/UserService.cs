using System.Data;
using System.Data.SqlClient;
using SANATIFY.Data;

namespace SANATIFY.Services;

public class UserService
{
    private readonly AppDbContext _appDbContext;

    public UserService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public void RegisterUser(string username, string passwordHash)
    {
        string query = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
        var parameters = new[]
        {
            new SqlParameter("@Username", username),
            new SqlParameter("@PasswordHash", passwordHash)
        };
        _appDbContext.ExecuteNonQuery(query, parameters);
    }

    public DataTable GetUser(string username)
    {
        var parameters = new[] { new SqlParameter("@Username", username) };
        string query = "SELECT * FROM Users WHERE Username = @Username";
        return _appDbContext.ExecuteQuery(query, parameters);
    }
}