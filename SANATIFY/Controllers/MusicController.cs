using Microsoft.AspNetCore.Mvc;
using SANATIFY.Data;
using SANATIFY.Models;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace SANATIFY.Controllers
{
    public class MusicController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public MusicController(IConfiguration configuration)
        {
            _configuration = configuration;
            _context = new AppDbContext(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
        public IActionResult DisplayAllMusics()
        {
            string query = "SELECT ID, Name, Person_ID, Genre_ID, Region, Ages, Date, Text, Playlist_Allow, Cover FROM Music";
            DataTable result = _context.ExecuteQuery(query, new SqlParameter[] { });

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
                    Ages = row["Ages"] != DBNull.Value ? (int?)row["Ages"] : null,
                    Date = (DateTime)row["Date"],
                    Text = row["Text"].ToString(),
                    Playlist_Allow = (bool)row["Playlist_Allow"],
                    Cover = (int)row["Cover"]
                });
            }

            return View(musicList);
        }
    }
}