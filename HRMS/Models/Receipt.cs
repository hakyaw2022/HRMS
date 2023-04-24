using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class Receipt
    {
        public int Id { get; set; }
        [ForeignKey("GuestId")]
        public Guest? Guest { get; set; }
        public int? GuestId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
        public int? RoomId { get; set; }
        [ForeignKey("AgentId")]
        public Agent? Agent { get; set; }
        public int? AgentId { get; set; }
        public string? Name { get; set; }
        public int Amount { get; set; }
        [DisplayName("Received Date")]
        public DateTime CreatedDate { get; set; }
        [DisplayName("Receipt Number")]
        public string? ReceiptNumber { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}