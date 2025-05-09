using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Theatre.Core.Models
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Login { get; set; }
        public static string Email { get; set; }
        public static int TypeId { get; set; }
        public static decimal Balance { get; set; }

    }
}
