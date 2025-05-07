using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBISX_POS.API.Models
{
    public class Timestamp
    {
        [Key]
        public int Id { get; set; }

        // The time when the cashier clocked in.
        public DateTimeOffset? TsIn { get; set; }

        // The time when the cashier clocked out.
        public DateTimeOffset? TsOut { get; set; }

        public decimal? CashInDrawerAmount { get; set; } = 0;
        public decimal? CashOutDrawerAmount { get; set; } = 0;
        //public decimal? CashWithdrawAmount { get; set; } = 0;
        public bool IsTrainMode { get; set; } = false;
        public ICollection<UserLog> ManagerLog { get; set; }
            = new List<UserLog>(); 
        
        // The cashier associated with this timestamp record (required).
        [Required]
        public required virtual User Cashier { get; set; }

        // The manager who authorized the clock-in (required).
        [Required]
        public User ManagerIn { get; set; }

        // The manager who authorized the clock-out.
        public User? ManagerOut { get; set; }

        // Computes the net work duration (clock-out minus clock-in, minus break duration if provided).
        [NotMapped]
        public TimeSpan? NetWorkDuration => (TsIn.HasValue && TsOut.HasValue) ? TsOut - TsIn : null;

    }
}
