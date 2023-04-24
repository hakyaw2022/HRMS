using System.ComponentModel;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HRMS.Models
{
    
    public enum BookingStatus
    {
        Booked,
        BookedEarlyCheckIn,        
        BookedLateCheckOut,       
        BookedEarlyCheckInAndLateCheckOut,
        BookedHourly,
        Ready,
        CheckedIn,
        EarlyCheckedIn,
        CheckedInToLateCheckOut,
        EarlyCheckedInToLateCheckOut,
        CheckedOut,        
        LateCheckedOut,
        CheckedInHourly,
        CheckedOutHourly,
        Cancelled,
        BookedService,
        Finished
    }
    
    public class Booking
    {
        [Required]
        public DateTime GetDateTime => DateTime.Now;
        public int Id { get; set; }       
        public Room? Room { get; set; }
        [DisplayName("Room Number")]
        [Required]
        public int RoomId { get; set; } 
        public int RoomPrice { get; set; }
        public Guest? Guest { get; set; }
        [Required]
        [DisplayName("Guest Name")]
        public int GuestId { get; set; }
        [Required(ErrorMessage = "Please select Booking Date - From")]
        [Display(Name = "Booking Date - From")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime From { get; set; }
        [Required(ErrorMessage = "Please select Booking Date - To")]
        [Display(Name = "Booking Date - To")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime To { get; set; }
        [DisplayName("Booking Status")]
        public BookingStatus BookingStatus { get; set; }
        public string? Comment { get; set; }
      
        public Agent? Agent { get; set; }
        [DisplayName("Agent Name")]
        public int? AgentId { get; set; }
        [DefaultValue(false)]
        public bool RoomStatusUpdate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}