using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OECD_SDMX
{
    public class Obs
    {
        private string myDate;
        private string myValue;

        public string obsDate
        {
            get
            {
                return myDate;
            }
            set
            {
                myDate = value;
            }
        }

        public string obsValue
        {
            get
            {
                return myValue;
            }
            set
            {
                myValue = value;
            }
        }
    }
}