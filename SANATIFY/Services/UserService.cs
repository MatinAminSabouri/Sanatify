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

        public void SendFriendRequest(int senderId, int receiverId)
        {
            string query =
                "INSERT INTO FriendRequest (SenderID, ReceiverID, Status, Date) VALUES (@SenderId, @ReceiverId, 2, @Date)";
            var parameters = new[]
            {
                new SqlParameter("@SenderId", senderId),
                new SqlParameter("@ReceiverId", receiverId),
                new SqlParameter("@Date", DateTime.Now)
            };

            _appDbContext.ExecuteNonQuery(query, parameters);
        }

        public List<FriendRequestViewModel> GetSentFriendRequests(int senderId)
        {
            string query =
                "SELECT ID, SenderID, ReceiverID, Status, Date FROM FriendRequest WHERE SenderID = @SenderId";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@SenderId", senderId)
            };

            DataTable result = _appDbContext.ExecuteQuery(query, parameters);

            List<FriendRequestViewModel> requests = new List<FriendRequestViewModel>();

            foreach (DataRow row in result.Rows)
            {
                requests.Add(new FriendRequestViewModel
                {
                    ID = Convert.ToInt32(row["ID"]),
                    SenderID = Convert.ToInt32(row["SenderID"]),
                    ReceiverID = Convert.ToInt32(row["ReceiverID"]),
                    Status = Convert.ToInt32(row["Status"]),
                    Date = Convert.ToDateTime(row["Date"])
                });
            }

            return requests;
        }

        public List<FriendRequestViewModel> GetReceivedFriendRequests(int receiverId)
        {
            string query = @"
        SELECT fr.ID, fr.SenderID, p.UserName AS SenderUserName, fr.Date
        FROM FriendRequest fr
        INNER JOIN Person p ON fr.SenderID = p.ID
        WHERE fr.ReceiverID = @ReceiverId AND fr.Status = 2"; // Assuming Status 2 means Pending

            var parameters = new[]
            {
                new SqlParameter("@ReceiverId", receiverId)
            };

            DataTable result = _appDbContext.ExecuteQuery(query, parameters);

            List<FriendRequestViewModel> requests = new List<FriendRequestViewModel>();

            foreach (DataRow row in result.Rows)
            {
                requests.Add(new FriendRequestViewModel
                {
                    ID = (int)row["ID"],
                    SenderID = (int)row["SenderID"],
                    SenderName = row["SenderUserName"].ToString(),
                    Date = Convert.ToDateTime(row["Date"])
                });
            }

            return requests;
        }

        public void AcceptFriendRequest(int requestId)
        {
            string updateQuery = "UPDATE FriendRequest SET Status = 1 WHERE ID = @RequestId";
            var updateParameters = new[]
            {
                new SqlParameter("@RequestId", requestId)
            };

            _appDbContext.ExecuteNonQuery(updateQuery, updateParameters);

            // Retrieve sender and receiver IDs from the request
            string selectQuery = "SELECT SenderID, ReceiverID FROM FriendRequest WHERE ID = @RequestId";
            var selectParameters = new[]
            {
                new SqlParameter("@RequestId", requestId)
            };

            DataTable result = _appDbContext.ExecuteQuery(selectQuery, selectParameters);

            if (result.Rows.Count > 0)
            {
                int senderId = (int)result.Rows[0]["SenderID"];
                int receiverId = (int)result.Rows[0]["ReceiverID"];

                // Insert into Friend table (assuming bidirectional friendship)
                string insertQuery =
                    "INSERT INTO Friend (Person1ID, Person2ID) VALUES (@Person1ID, @Person2ID), (@Person2ID, @Person1ID)";
                var insertParameters = new[]
                {
                    new SqlParameter("@Person1ID", senderId),
                    new SqlParameter("@Person2ID", receiverId)
                };

                _appDbContext.ExecuteNonQuery(insertQuery, insertParameters);
            }
            else
            {
                throw new Exception("Friend request not found.");
            }
        }

        public void RejectFriendRequest(int requestId)
        {
            string updateQuery = "UPDATE FriendRequest SET Status = 0 WHERE ID = @RequestId";
            var updateParameters = new[]
            {
                new SqlParameter("@RequestId", requestId)
            };

            _appDbContext.ExecuteNonQuery(updateQuery, updateParameters);
        }

        public List<UserViewModel> GetFriends(int userId)
        {
            string query = @"
        SELECT p.ID, p.UserName, p.Email
        FROM Person p
        INNER JOIN Friend f ON (p.ID = f.Person1ID OR p.ID = f.Person2ID)
        WHERE (f.Person1ID = @UserId OR f.Person2ID = @UserId)
          AND p.ID != @UserId";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId)
            };

            DataTable result = _appDbContext.ExecuteQuery(query, parameters);

            List<UserViewModel> friends = new List<UserViewModel>();

            foreach (DataRow row in result.Rows)
            {
                friends.Add(new UserViewModel
                {
                    ID = (int)row["ID"],
                    UserName = row["UserName"].ToString(),
                    Email = row["Email"].ToString()
                });
            }

            return friends;
        }

        public void Unfriend(int userId, int friendId)
        {
            string query = "DELETE FROM Friend WHERE (Person1ID = @UserId AND Person2ID = @FriendId) " +
                           "OR (Person1ID = @FriendId AND Person2ID = @UserId)";
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@FriendId", friendId)
            };

            _appDbContext.ExecuteNonQuery(query, parameters);
        }

        public List<MessageViewModel> GetChatMessages(int userId, int friendId)
        {
            string query = @"
        SELECT m.ID, m.Sender_ID, m.Rec_ID, m.Text, m.date, p.UserName as SenderName
        FROM Message m
        JOIN Person p ON m.Sender_ID = p.ID
        WHERE (m.Sender_ID = @UserId AND m.Rec_ID = @FriendId) OR (m.Sender_ID = @FriendId AND m.Rec_ID = @UserId)
        ORDER BY m.date";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@FriendId", friendId)
            };

            DataTable result = _appDbContext.ExecuteQuery(query, parameters);
            List<MessageViewModel> messages = new List<MessageViewModel>();

            foreach (DataRow row in result.Rows)
            {
                messages.Add(new MessageViewModel
                {
                    ID = (int)row["ID"],
                    SenderID = (int)row["Sender_ID"],
                    RecID = (int)row["Rec_ID"],
                    SenderName = row["SenderName"].ToString(),
                    Text = row["Text"].ToString(),
                    Date = (DateTime)row["date"]
                });
            }

            return messages;
        }

        public void SendMessage(int userId, int friendId, string message)
        {
            string query =
                "INSERT INTO Message (Sender_ID, Rec_ID, Text, date) VALUES (@SenderID, @RecID, @Text, @Date)";
            var parameters = new[]
            {
                new SqlParameter("@SenderID", userId),
                new SqlParameter("@RecID", friendId),
                new SqlParameter("@Text", message),
                new SqlParameter("@Date", DateTime.Now)
            };

            _appDbContext.ExecuteNonQuery(query, parameters);
        }

        public void AddComment(int personId, int musicId, string text)
        {
            string query =
                "INSERT INTO Comment_Music (Person_ID, Music_ID, Text) VALUES (@Person_ID, @Music_ID, @Text)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Person_ID", personId),
                new SqlParameter("@Music_ID", musicId),
                new SqlParameter("@Text", text)
            };
            _appDbContext.ExecuteNonQuery(query, parameters);
        }

        public List<CommentViewModel> GetCommentsForMusic(int musicId)
        {
            string query = @"
            SELECT cm.ID, cm.Text, p.UserName AS PersonName
            FROM Comment_Music cm
            JOIN Person p ON cm.Person_ID = p.ID
            WHERE cm.Music_ID = @Music_ID
            ORDER BY cm.ID DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Music_ID", musicId)
            };

            DataTable result = _appDbContext.ExecuteQuery(query, parameters);
            List<CommentViewModel> comments = new List<CommentViewModel>();

            foreach (DataRow row in result.Rows)
            {
                comments.Add(new CommentViewModel
                {
                    ID = (int)row["ID"],
                    Text = row["Text"].ToString(),
                    PersonName = row["PersonName"].ToString()
                });
            }

            return comments;
        }

        public List<MusicViewModel> SearchMusics(string searchBy, string searchTerm)
        {
            string query = "";

            switch (searchBy.ToLower())
            {
                case "name":
                    query = "SELECT * FROM Music WHERE LOWER(Name) LIKE @SearchTerm";
                    break;
                case "artistname":
                    query = "SELECT * FROM Music WHERE LOWER(Person_ID) LIKE @SearchTerm";
                    break;
                case "genre":
                    query = "SELECT * FROM Music WHERE LOWER(Genre_ID) LIKE @SearchTerm";
                    break;
                case "age":
                    query = "SELECT * FROM Music WHERE LOWER(Ages) LIKE @SearchTerm";
                    break;
                default:
                    query = "SELECT * FROM Music WHERE LOWER(Name) LIKE @SearchTerm";
                    break;
            }

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@SearchTerm", "%" + searchTerm.ToLower() + "%")
            };

            DataTable result = _appDbContext.ExecuteQuery(query, parameters);
            List<MusicViewModel> musics = new List<MusicViewModel>();

            foreach (DataRow row in result.Rows)
            {
                musics.Add(new MusicViewModel
                {
                    ID = (int)row["ID"],
                    Name = row["Name"].ToString(),
                    Person_ID = (int)row["Person_ID"],
                    Genre_ID = (int)row["Genre_ID"],
                    Region = row["Region"].ToString(),
                    Ages = (int)row["Ages"],
                    Date = (DateTime)row["Date"],
                    Cover = (int)row["Cover"]
                });
            }

            return musics;
        }

        public void AddConcert(int personId, DateTime date, int price)
        {
            string query = @"INSERT INTO Concert (Person_ID, Date, Price)
                             VALUES (@Person_ID, @Date, @Price)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Person_ID", personId),
                new SqlParameter("@Date", date),
                new SqlParameter("@Price", price)
            };

            _appDbContext.ExecuteNonQuery(query, parameters);
        }

        public void CancelConcertAndRefund(int concertId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ConcertId", concertId)
                };
                Console.WriteLine("We are in cancle");
                var result = _appDbContext.ExecuteNonQuery("CancelConcertAndRefundTheUsers @ConcertId", parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new ApplicationException("Failed to cancel concert and refund tickets.", ex);
            }
        }
        public List<MusicViewModel> GetLikedSongs(int userId)
        {
            try
            {
                // Query to fetch liked songs
                string query = @"
            SELECT m.ID, m.Name, m.Person_ID, m.Genre_ID, m.Region, m.Ages, m.Date, m.Text, m.Playlist_Allow, m.Cover
            FROM Music m
            INNER JOIN Like_Music lm ON m.ID = lm.Music_ID
            WHERE lm.Person_ID = @UserId";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId)
                };

                // Execute the query and fetch results
                var result = _appDbContext.ExecuteQuery(query, parameters);

                // Convert DataTable rows to MusicViewModel objects
                var likedSongs = new List<MusicViewModel>();
                foreach (DataRow row in result.Rows)
                {
                    likedSongs.Add(new MusicViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Name = row["Name"].ToString(),
                        Person_ID = Convert.ToInt32(row["Person_ID"]),
                        Genre_ID = Convert.ToInt32(row["Genre_ID"]),
                        Region = row["Region"].ToString(),
                        Ages = Convert.ToInt32(row["Ages"]),
                        Date = Convert.ToDateTime(row["Date"]),
                        Text = row["Text"].ToString(),
                        Playlist_Allow = Convert.ToBoolean(row["Playlist_Allow"]),
                        Cover = Convert.ToInt32(row["Cover"])
                    });
                }

                return likedSongs;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve liked songs.", ex);
            }
        }

        public int GetMostLikedGenreId(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId)
                };

                var result = _appDbContext.ExecuteQuery("GetMostLikedGenre @UserId", parameters);
                if (result.Rows.Count > 0)
                {
                    return (int)result.Rows[0]["GENRE"];
                }
                else
                {
                    throw new ApplicationException("User's most liked genre not found.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get user's most liked genre.", ex);
            }
        }


        public List<MusicViewModel> GetRecommendedSongs(int genreId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@GenreId", genreId)
                };

                var result = _appDbContext.ExecuteQuery("GetRecommendedSongsByGenre @GenreId", parameters);

                // Map the result to MusicViewModel list
                List<MusicViewModel> recommendedSongs = new List<MusicViewModel>();
                foreach (DataRow row in result.Rows)
                {
                    recommendedSongs.Add(new MusicViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Name = row["Name"].ToString(),
                        Person_ID = Convert.ToInt32(row["Person_ID"]),
                        Genre_ID = Convert.ToInt32(row["Genre_ID"]),
                        Region = row["Region"].ToString(),
                        Ages = row["Ages"] != DBNull.Value ? Convert.ToInt32(row["Ages"]) : (int?)null,
                        Date = Convert.ToDateTime(row["Date"]),
                        Text = row["Text"].ToString(),
                        Playlist_Allow = Convert.ToBoolean(row["Playlist_Allow"]),
                        Cover = Convert.ToInt32(row["Cover"])
                    });
                }

                return recommendedSongs;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get recommended songs.", ex);
            }
        }
        
        public void AddMusicToPlaylist(int playlistId, int musicId)
        {
            try
            {
                var query = "INSERT INTO Music_Playlist (Music_ID, Playlist_ID) VALUES (@MusicId, @PlaylistId)";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@MusicId", musicId),
                    new SqlParameter("@PlaylistId", (int)1)
                };

                _appDbContext.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to add music to playlist.", ex);
            }
        }

        public List<MusicViewModel> GetAllMusic()
        {
            var query = " SELECT * FROM Music WHERE Playlist_Allow = @Allow ";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Allow", 1)
            };
            var result = _appDbContext.ExecuteQuery(query, parameters);

            var musicList = new List<MusicViewModel>();

            foreach (DataRow row in result.Rows)
            {
                musicList.Add(new MusicViewModel
                {
                    ID = (int)row["ID"],
                    Name = row["Name"].ToString(),
                    Person_ID = (int)row["Person_ID"],
                    Genre_ID = (int)row["Genre_ID"],
                    Region = row["Region"].ToString(),
                    Ages = (int)row["Ages"],
                    Date = (DateTime)row["Date"],
                    Text = row["Text"].ToString(),
                    Playlist_Allow = (bool)row["Playlist_Allow"],
                    Cover = (int)row["Cover"]
                });
            }

            return musicList;
        }
        public IEnumerable<MusicViewModel> GetMusicInPlaylist(int playlistId)
        {
            try
            {
                var query = @"
                    SELECT m.ID, m.Name, m.Person_ID, m.Genre_ID, m.Region, m.Ages, m.Date, m.Text, m.Playlist_Allow, m.Cover
                    FROM Music m
                    JOIN Music_Playlist mp ON m.ID = mp.Music_ID
                    WHERE mp.Playlist_ID = @PlaylistId
                ";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@PlaylistId", playlistId)
                };

                var result = _appDbContext.ExecuteQuery(query, parameters);

                var musicList = new List<MusicViewModel>();

                foreach (DataRow row in result.Rows)
                {
                    musicList.Add(new MusicViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Name = row["Name"].ToString(),
                        Person_ID = Convert.ToInt32(row["Person_ID"]),
                        Genre_ID = Convert.ToInt32(row["Genre_ID"]),
                        Region = row["Region"].ToString(),
                        Ages = Convert.ToInt32(row["Ages"]),
                        Date = Convert.ToDateTime(row["Date"]),
                        Text = row["Text"].ToString(),
                        Playlist_Allow = Convert.ToBoolean(row["Playlist_Allow"]),
                        Cover = Convert.ToInt32(row["Cover"])
                    });
                }

                return musicList;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve music in playlist.", ex);
            }
        }public List<MusicViewModel> GetMusicForPlaylist()
        {
            try
            {
                // Query to fetch music that can be added to playlist
                string query = "SELECT ID, Name, Person_ID, Genre_ID, Region, Ages, Date, Text, Playlist_Allow, Cover FROM Music WHERE Playlist_Allow = @Allow ";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Allow", (int)1)
                };
                var result = _appDbContext.ExecuteQuery(query, parameters);

                // Convert DataTable rows to MusicViewModel objects
                var musicList = new List<MusicViewModel>();
                foreach (DataRow row in result.Rows)
                {
                    musicList.Add(new MusicViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Name = row["Name"].ToString(),
                        Person_ID = Convert.ToInt32(row["Person_ID"]),
                        Genre_ID = Convert.ToInt32(row["Genre_ID"]),
                        Region = row["Region"].ToString(),
                        Ages = Convert.ToInt32(row["Ages"]),
                        Date = Convert.ToDateTime(row["Date"]),
                        Text = row["Text"].ToString(),
                        Playlist_Allow = Convert.ToBoolean(row["Playlist_Allow"]),
                        Cover = Convert.ToInt32(row["Cover"])
                    });
                }

                return musicList;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve music for playlist.", ex);
            }
        }
        public void AddMusicToPlaylist(int userId, List<int> musicIds)
        {
            try
            {
                foreach (int musicId in musicIds)
                {
                    // Insert into Music_Playlist table
                    string query = "INSERT INTO Music_Playlist (Music_ID, Playlist_ID) VALUES (@MusicId, @PlaylistId)";
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@MusicId", musicId),
                        new SqlParameter("@PlaylistId", userId) // Example: Using user ID as Playlist ID
                    };

                    _appDbContext.ExecuteNonQuery(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to add music to playlist.", ex);
            }
        }

    }
}