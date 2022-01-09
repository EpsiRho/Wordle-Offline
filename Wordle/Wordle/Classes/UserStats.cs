using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle.Classes
{
    public static class UserStats
    {
        public static double Played { get; set; }
        public static double WinNum { get;set; }
        public static int CurrentStreak { get; set; }
        public static int MaxStreak { get; set; }
        public static List<double> distro { get; set; }
    }
}
