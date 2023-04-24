using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using HRMS.Helpers;

namespace HRMS.Models
{
    public class Guest
    {
        private const int V = 1;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Phone { get; set; }
        [Required]
        public string? Address { get; set; }
        
        [Required]
        [Display(Name = "NRC or Passport")]
        public string? NRC { get; set; }
        public string? Photo1Name { get; set; }
        public string? Photo2Name { get; set; }
        public string? Photo3Name { get; set; }
        [NotMapped]
        public IFormFile? Photo1 { get; set; }
        [NotMapped]
        public IFormFile? Photo2 { get; set; }
        [NotMapped]
        public IFormFile? Photo3 { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
