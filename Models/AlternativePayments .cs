using System.ComponentModel.DataAnnotations;

namespace EBISX_POS.API.Models
{
    public class AlternativePayments
    {
        [Key]
        public long Id { get; set; }
        public required string Reference { get; set; }
        public required decimal Amount { get; set; }
        public required Order Order { get; set; }
        public required SaleType SaleType { get; set; }
    }
}
