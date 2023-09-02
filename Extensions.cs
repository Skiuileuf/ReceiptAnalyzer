using System;

namespace LidlPriceStats
{
    public static class Extensions
    {
        public static int? IntTryParse(this string Source)
        {
            int result;
            if (int.TryParse(Source, out result))
                return result;
            else

                return null;
        }

    }
}
