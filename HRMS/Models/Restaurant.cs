using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class Restaurant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }

        [DisplayName("Item Code")]
        public string? ItemCode { get; set; }
        public int? Quantity { get; set; }
        public int UnitPrice { get; set; }
        [DisplayName("Menu Category")]
        public string? Category { get; set; }

    }
}