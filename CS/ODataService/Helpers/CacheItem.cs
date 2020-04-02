using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataService.Helpers
{

    public class CacheItem
    {
        public SecurityStrategyComplex Security { get; set; }

        public SecuredObjectSpaceProvider Provider { get; set; }

        public CacheItem()
        {

        }
    }
}