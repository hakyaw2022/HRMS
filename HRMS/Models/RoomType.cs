using System.ComponentModel;

namespace HRMS.Models
{  
    public class RoomType
    {
        public int Id { get; set; }
        [DisplayName("Room Type")]
        public string? Name { get; set; }        
        public int Price { get; set; }
        [DisplayName("Hourly Price")]
        public int HourlyPrice { get; set; }
    }
}