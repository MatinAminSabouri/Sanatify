namespace SANATIFY.Models;

public class MusicViewModel
{
    public int ID { get; set; }
    public string? Name { get; set; }
    public int Person_ID { get; set; }
    public int Genre_ID { get; set; }
    public string? Region { get; set; }
    public int? Ages { get; set; }
    public DateTime Date { get; set; }
    public string? Text { get; set; }
    public bool Playlist_Allow { get; set; }
    public int Cover { get; set; }
}