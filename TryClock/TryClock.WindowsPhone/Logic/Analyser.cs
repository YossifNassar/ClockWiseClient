using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryClock.Logic
{
    public class Analyser
    {
        public static double AnalyseMovement(double d)
        {
            if (d > 1)
            {
                return 1;
            }
            if (d < 0)
            {
                return 0;
            }
            return d;
        }

        public static int AnalyseHeartRate(int d)
        {
            if (d > 70)
            {
                return 70;
            }
            if (d < 58)
            {
                return 58;
            }
            return d;
        }
    }
}
