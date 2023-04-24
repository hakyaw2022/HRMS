using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HRMS.Models
{
    public enum ServiceStatus
    {
        Available,
        Unavailable
    }

    public enum PricingType
    {
        Session,
        Hour
    }

    public partial class Service
    {        
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        [DisplayName("Pricing Type")]
        public PricingType PricingType { get; set; }
        public ServiceStatus Status { get; set; }
        public int Capacity { get; set; }

        [DisplayName("Opened Time")]        
        public string? OpenedTime { get; set; }
        [DisplayName("Closed Time")]        
        public string? ClosedTime { get; set; }        

    }
}