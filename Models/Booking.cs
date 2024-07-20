using System;
using System.Collections.Generic;

namespace PRN221_SE1729_Group11_Project.Models
{
    public partial class Booking
    {
        public int BookingId { get; set; }
        public int? Cid { get; set; }
        public int? Pid { get; set; }
        public DateTime? RentDate { get; set; }
        public int? PayStatus { get; set; }
        public int? BookingStatus { get; set; }
        public string? PayProof { get; set; }
        public string? Note { get; set; }

        public virtual Customer? CidNavigation { get; set; }
        public virtual Product? PidNavigation { get; set; }
    }
}
