using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public enum RoomStatus
    {
        Available,
        Reserved,
        Occupied,
        ReservedHourly,
        Unknown
    }
    public class Rooms
    {
        public int From { get; set; }
        public int To { get; set; }
    }
    public class Room
    {
        public int Id { get; set; }
        public RoomType? RoomType { get; set; }
        [DisplayName("Room Type")]        
        public int RoomTypeId { get; set; }
        [DisplayName("Room Number")]
        
        public int RoomNumber { get; set; }
        [DisplayName("Room Status")]
        public RoomStatus RoomStatus { get; set; }
        public Guest? Guest  { get; set; }
        public int? GuestId { get; set; }

        // these properties are not mapped to database
        [NotMapped]
        public DateTime? CurrentBookedFrom { get; set; }
        [NotMapped]
        public DateTime? CurrentBookedTo { get; set; }

        [NotMapped]
        public CheckedInCustomer? CheckedInCustomer { get; set; }
    }
}