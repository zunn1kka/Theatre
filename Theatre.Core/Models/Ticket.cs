using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Core.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int SeatId { get; set; }
        public Seat Seat { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime BookingTime { get; set; } = DateTime.Now;
    }
}
