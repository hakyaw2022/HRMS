using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HRMS.Models
{
    public class Agent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Phone { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        [Display(Name = "Company License")]
        public string? NRC { get; set; }
        public string? Photo1Name { get; set; }
        public string? Photo2Name { get; set; }
        public string? Photo3Name { get; set; }
        [NotMapped]
        [Display(Name = "Photo-1")]
        public IFormFile? Photo1 { get; set; }
        [NotMapped]
        [Display(Name = "Photo-2")]
        public IFormFile? Photo2 { get; set; }
        [NotMapped]
        [Display(Name = "Photo-3")]
        public IFormFile? Photo3 { get; set; }
        public DateTime CreatedDate { get; set; }
    }
   
}
