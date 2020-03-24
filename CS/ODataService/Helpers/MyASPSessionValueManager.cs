using DevExpress.ExpressApp.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataService.Helpers
{
    public class MyASPSessionValueManager<T> : ASPSessionValueManager<T>
    {
        public MyASPSessionValueManager(string key) : base(key)
        {
        }
    }
}