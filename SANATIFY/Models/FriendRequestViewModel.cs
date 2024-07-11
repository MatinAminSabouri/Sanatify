namespace SANATIFY.Models;

public class FriendRequestViewModel
{
    public int ID { get; set; } // Assuming this is for FriendRequest ID
    public int SenderID { get; set; }
    public string SenderName { get; set; } // Assuming you want to display sender's name in UI
    public int ReceiverID { get; set; }
    public string ReceiverName { get; set; } // Assuming you want to display receiver's name in UI
    public int Status { get; set; } // 0 = REJECTED, 1 = ACCEPTED, 2 = PENDING
    public DateTime Date { get; set; } // Date of the request
}