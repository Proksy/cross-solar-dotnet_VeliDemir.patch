using System.ComponentModel.DataAnnotations;

namespace CrossSolar.Models
{
    public class PanelModel
    {
        public int Id { get; set; }

        [Required]
        [Range(-90, 90)]
        //[RegularExpression(@"^\d+(\.\d{6})$")]
        [RegularExpression(@"^[0-9]{1,2}.[0-9]{6}$")]
        public double Latitude { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{1,2}.[0-9]{6}$")]
        [Range(-180, 180)] public double Longitude { get; set; }

        [Required]
        [StringLength(16)]
        public string Serial { get; set; }

        [Required]
        public string Brand { get; set; }
    }
}