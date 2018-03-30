using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Common
{
   public class DataPager
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public int Total { get; set; }
        public string KeyWord { get; set; }
    }
}
