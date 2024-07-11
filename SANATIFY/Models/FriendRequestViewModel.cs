namespace SANATIFY.Models;

public class FriendRequestViewModel
{
    public int ID { get; set; }
    public int Person_Sender_ID { get; set; }
    public int Person_Rec_ID { get; set; }
    public bool Accept { get; set; }
    public bool State { get; set; }
    public DateTime Date { get; set; }
}