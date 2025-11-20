using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaginationApi.Models
{
    [Table("data_small")]
    public class TaxiDrivings : BaseEntity
    {
        [Key]
       public int id { get; set; }
       public string? medallion { get; set; }
       public string? hashLicense { get; set; }
       public DateTime? pickupTime { get; set; }
       public DateTime? dropOffTime { get; set; }
       public short? duration { get; set; }
       public double? distance { get; set; }
       public double? pLongitude { get; set; }
       public double? pLatitude { get; set; }
       public double? dLongitude { get; set; }
       public double? dLatitude { get; set; }
       public string? paymentType { get; set; }
       public double? fareAmount { get; set; }
       public double? surchange { get; set; }
       public double? tax { get; set; }
       public double? tipAmount { get; set; }
       public double? tollsAmount { get; set; }
       public double? totalAmount { get; set; }
      


    }
}
