using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.UserCenter.Controllers
{
    public class BasicsController : Controller
    {
        protected NameValueCollection FormatQueryString(NameValueCollection nameValues)
        {
            NameValueCollection nvcollection = new NameValueCollection();
            foreach (var item in nameValues.AllKeys)
            {
                if (item.Equals("df"))
                {
                    string daterange = nameValues["dr"];
                    if (!String.IsNullOrEmpty(daterange))
                    {
                        if (daterange.Contains('-'))
                        {
                            DateTime startDate = daterange.Split('-')[0].Trim().ToDateTime().AddSeconds(-1);
                            DateTime endDate = daterange.Split('-')[1].Trim().ToDateTime().AddDays(1);
                            nvcollection.Add(nameValues[item] + ">", startDate.ToString());
                            nvcollection.Add(nameValues[item] + "<", endDate.ToString());
                        }
                    }
                }
                else if (item.Equals("kf"))
                {
                    if (!string.IsNullOrEmpty(nameValues["kv"]))
                        nvcollection.Add(nameValues[item], nameValues["kv"]);
                }
                else if (item.Equals("isasc") || !string.IsNullOrEmpty(nameValues["isasc"]))
                    nvcollection.Add(item, nameValues["isasc"]);
                else if (item.Equals("orderfiled") || !string.IsNullOrEmpty(nameValues["orderfiled"]))
                    nvcollection.Add(item, nameValues["orderfiled"]);
            }

            return nvcollection;
        }
    }
}