using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Theatre.Core.Models
{
    public class Spectacle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int QuantityActors { get; set; }
        public BitmapImage Image { get; set; }
        public List<Genre> Genres { get; set; } = new List<Genre>();
    }
}
