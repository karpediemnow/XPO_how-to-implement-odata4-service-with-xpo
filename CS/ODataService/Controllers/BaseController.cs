using DevExpress.ExpressApp;
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
        private SecuredObjectSpaceProvider provider = null;
        public SecuredObjectSpaceProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    provider = ConnectionHelper.GetSecuredObjectSpaceProvider();
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
                if (provider != null)
                {
                    provider.Dispose();
                    provider = null;
                }
            }
        }

    }
}