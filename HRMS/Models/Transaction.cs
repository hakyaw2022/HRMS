using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public enum TransactionType
    {
        Room,
        Service,
        Restaurant,
        RoomDeposit
    }
    public enum TransactionStatus
    {
        Paid,
        Active,
        Inactive
    }
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus TransactionStatus { get; set; }        
        public Room? Room { get; set; }
        public int? RoomId { get; set; }
        public int Amount { get; set; }
        public int Quantity { get; set; }
        public int SubTotal { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Description { get;set; }
        public Guest? Guest { get; set; }   
        public Agent? Agent { get; set; }
        public Restaurant? Restaurant { get; set;}
        public Service? Service { get; set; }
    }
}