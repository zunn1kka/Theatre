using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theatre.Core.Models;

namespace Theatre.Data
{
    public class DatabaseContext
    {
        public DbSet<User> Users { get; set; }
    }
}
