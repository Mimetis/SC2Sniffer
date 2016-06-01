using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace SC2MiM.Common.Helpers
{
    public static class Tools
    {
        public static Boolean TryParse(string s, out int result)
        {
            return Int32.TryParse(s, NumberStyles.Any, CultureHelper.DefaultCultureInfo, out result);
        }


        public static ParallelOptions GetParallelOptions()
        {
#if DEBUG
            return new ParallelOptions() { MaxDegreeOfParallelism = 1 };
#else
             return new ParallelOptions() ;
#endif
        }
    }
}
