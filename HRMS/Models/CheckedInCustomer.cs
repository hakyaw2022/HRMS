using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class CheckedInCustomer
    {
        public int Id { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
        
        public int RoomId { get; set; }
        [ForeignKey("GuestId")]
        public Guest? Guest { get; set; }
        public int GuestId { get; set; }
        public DateTime checkedInTimeStamp { get; set; }
    }
}
