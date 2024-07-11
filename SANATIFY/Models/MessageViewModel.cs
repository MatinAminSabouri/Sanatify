namespace SANATIFY.Models;

public class MessageViewModel
{
    public int ID { get; set; }
    public int SenderID { get; set; }
    public int RecID { get; set; }
    public string SenderName { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }
}