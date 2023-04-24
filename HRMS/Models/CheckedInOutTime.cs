using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class CheckedInOutTime
    {
        public int Id { get; set; }
        [ForeignKey("RoomTypeId")]

        public RoomType? RoomType { get; set; }
        [DisplayName("Room Type")]
        public int RoomTypeId { get; set; }
              
        [DisplayName("Checked In Time")]        
        public string? CheckedInTime { get; set; }
        
        [DisplayName("Checked Out Time")]
        public string? CheckedOutTime { get; set; }
    }
}
