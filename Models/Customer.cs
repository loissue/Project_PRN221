using System;
using System.Collections.Generic;

namespace PRN221_SE1729_Group11_Project.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Bookings = new HashSet<Booking>();
        }

        public int Cid { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? Dob { get; set; }
        public string? IdentificationCard { get; set; }
        public string? Address { get; set; }
        public int? NumberOfOrder { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
