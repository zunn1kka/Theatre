using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Core.Models
{
    public class SpectacleGenre
    {
        public int SpectacleId { get; set; }
        public int GenreId { get; set; }
        public Spectacles Spectacle { get; set; }
        public Genre Genre { get; set; }
    }
}
