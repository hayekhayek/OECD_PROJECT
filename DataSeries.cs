using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feri.Data.Factory;
using Feri.Data.Access;
using System.Globalization;

namespace OECD_SDMX
{
    public class DataSeries
    {
        public string Description { get; set; }
        public string SeriesId { get; set; }
        public string Source { get; set; }
        public string Freq { get; set; }
        public string Start { get; set; }
        public int Decimals { get; set; }

        public Frequency Frequency
        {
            get
            {
                switch (Freq)
                {
                    case "M": return Frequency.Monthly;
                    case "Q": return Frequency.Quarterly;
                    case "H": return Frequency.Semiannual;
                    case "S": return Frequency.Semiannual;
                    case "D": return Frequency.Daily;
                    case "A": return Frequency.Annual;
                    default: return Frequency.None;
                }
            }
        }

        public DateTime StartDate
        {
            get
            {
                switch (Freq)
                {
                    case "M": return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), Convert.ToInt32(Start.Substring(4, 2), CultureInfo.CurrentCulture), 1);
                    case "H": return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), (Convert.ToInt32(Start.Substring(4, 2), CultureInfo.CurrentCulture) * 6) - 5, 1);
                    case "S": return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), (Convert.ToInt32(Start.Substring(4, 2), CultureInfo.CurrentCulture) * 6) - 5, 1);
                    case "T": return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), (Convert.ToInt32(Start.Substring(5, 1), CultureInfo.CurrentCulture) * 3) - 2, 1);
                    case "Q": return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), (Convert.ToInt32(Start.Substring(5, 1), CultureInfo.CurrentCulture) * 3) - 2, 1);
                    case "A": return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), 1, 1);
                    case "D": return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), Convert.ToInt32(Start.Substring(4, 2), CultureInfo.CurrentCulture), Convert.ToInt32(Start.Substring(8, 2), CultureInfo.CurrentCulture));
                    default: return new DateTime(Convert.ToInt32(Start.Substring(0, 4), CultureInfo.CurrentCulture), Convert.ToInt32(Start.Substring(4, 2), CultureInfo.CurrentCulture), 1);
                }
            }
        }
    }
}
