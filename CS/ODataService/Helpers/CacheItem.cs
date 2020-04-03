using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataService.Helpers
{

    public class CacheItem : IDisposable
    {
        public SecurityStrategyComplex Security { get; set; }

        public SecuredObjectSpaceProvider Provider { get; set; }

        public CacheItem()
        {

        }

        public void Dispose()
        {
            if (Security != null)
            {
                Security.Dispose();
                Security = null;
            }
            if (Provider != null)
            {
                Provider.Dispose();
                Provider = null;
            }
        }
    }
}