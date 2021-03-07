using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace OECD_SDMX
{
    public class Obslist
    {
        private List<Obs> myObsList;

        public List<Obs> ObsList
        {
            get
            {
                return myObsList;
            }
        }

        public void ObsListAscendingOrder()
        {
            myObsList = myObsList.OrderBy(o => o.obsDate).ToList();
        }

        public void ObsListDescendingOrder()
        {
            myObsList = myObsList.OrderByDescending(o => o.obsDate).ToList();
        }

        public void Add(Obs obs)
        {
            if (myObsList == null)
            {
                myObsList = new List<Obs>();
            }
            myObsList.Add(obs);
        }

        public int Count
        {
            get
            {
                return myObsList.Count();
            }
        }

        public bool isValue(int Index)
        {
            if (string.IsNullOrEmpty(myObsList[Index].obsValue))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Double itemValue(int Index)
        {
            string value = string.Empty;
            NumberStyles style = NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            Double number;

            value = myObsList[Index].obsValue;

            if (Double.TryParse(value, style, culture, out number))
                return number;
            else
                return 0;

        }
        public string itemDate(int Index)
        {
            return myObsList[Index].obsDate;
        }

    }
}
