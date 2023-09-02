using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidlPriceStats
{
    public class PricePoint
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }

        public PricePoint(DateTime date, decimal price)
        {
            this.Date = date;
            this.Price = price;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
