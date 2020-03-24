using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using ODataService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataService.Controllers
{
    public class BaseController : ODataController
    {
        private static int LastLogin = 0;

        private SecurityStrategyComplex security = null;

        private SecuredObjectSpaceProvider provider = null;
        public SecuredObjectSpaceProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    string userName = null;
                    if (LastLogin % 2 == 0)
                    {
                        userName = "User1";
                    }
                    else
                    {
                        userName = "User2";
                    }
                    LastLogin++;

                    security = ConnectionHelper.GetSecurity(userName);
                    provider = ConnectionHelper.GetSecuredObjectSpaceProvider(security);
                }
                return provider;
            }
        }

        private Session session = null;
        public Session Session
        {
            get
            {
                if (session == null)
                {
                    session = GetNewUow();
                }
                return session;
            }
        }

        public UnitOfWork GetNewUow()
        {
            var os = Provider.CreateObjectSpace();
            return (UnitOfWork)((XPObjectSpace)os).Session;
        }

        public IObjectSpace GetSecuredObjectSpace()
        {
            var os = Provider.CreateObjectSpace();
            return os;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (session != null)
                {
                    session.Dispose();
                    session = null;
                }
                if (security != null)
                {
                    security.Dispose();
                    security = null;
                }
                if (provider != null)
                {
                    provider.Dispose();
                    provider = null;
                }
            }
        }

    }
}