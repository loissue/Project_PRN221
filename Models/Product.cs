using System;
using System.Collections.Generic;

namespace PRN221_SE1729_Group11_Project.Models
{
    public partial class Product
    {
        public Product()
        {
            Bookings = new HashSet<Booking>();
        }

        public int Pid { get; set; }
        public string? ProductName { get; set; }
        public double? Price { get; set; }
        public double? RentPrice { get; set; }
        public int? RentedTime { get; set; }
        public string? Relate { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
