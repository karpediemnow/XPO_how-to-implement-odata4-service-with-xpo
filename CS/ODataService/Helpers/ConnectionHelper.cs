using System;
using DevExpress.Xpo;
using WebApplication1.Models;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using System.Configuration;
using DevExpress.ExpressApp.Xpo;
using System.Data;
using ODataService.Models;
using System.Linq;
using DevExpress.Persistent.Base;

namespace ODataService.Helpers
{

    public static class ConnectionHelper
    {
        static Type[] persistentTypes = new Type[] {
            typeof(BaseDocument),
            typeof(Customer),
            typeof(OrderDetail),
            typeof(Order),
            typeof(Contract),
            typeof(Product),

        };
        public static Type[] GetPersistentTypes()
        {
            Type[] copy = new Type[persistentTypes.Length];
            Array.Copy(persistentTypes, copy, persistentTypes.Length);
            return copy;
        }
        public static string ConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
        }
        public static UnitOfWork CreateSession()
        {
            return new UnitOfWork() { IdentityMapBehavior = IdentityMapBehavior.Strong };
        }
        public static IDataLayer CreateDataLayer(AutoCreateOption autoCreationOption, bool threadSafe)
        {
            XpoTypesInfoHelper.GetXpoTypeInfoSource();
            XafTypesInfo.Instance.RegisterEntity(typeof(Employee));
            XafTypesInfo.Instance.RegisterEntity(typeof(PermissionPolicyUser));
            XafTypesInfo.Instance.RegisterEntity(typeof(PermissionPolicyRole));

            foreach (var item in persistentTypes)
            {
                XafTypesInfo.Instance.RegisterEntity(item);
            }

            if (threadSafe)
            {
                var provider = XpoDefault.GetConnectionProvider(XpoDefault.GetConnectionPoolString(ConnectionString), autoCreationOption);
                return new ThreadSafeDataLayer(XpoTypesInfoHelper.GetXpoTypeInfoSource().XPDictionary, provider);
            }
            else
            {
                var provider = XpoDefault.GetConnectionProvider(ConnectionString, autoCreationOption);
                return new SimpleDataLayer(XpoTypesInfoHelper.GetXpoTypeInfoSource().XPDictionary, provider);
            }
        }
        public static void EnsureDatabaseCreated()
        {
            using (IDataLayer dataLayer = CreateDataLayer(AutoCreateOption.DatabaseAndSchema, false))
            {
                using (UnitOfWork uow = new UnitOfWork(dataLayer))
                {
                    uow.UpdateSchema();
                }

                using (UnitOfWork uow = new UnitOfWork(dataLayer))
                {
                    CreateUser(uow, "User1");
                    CreateUser(uow, "User2");
                    CreateOrder(uow);
                    uow.CommitChanges();
                    uow.PurgeDeletedObjects();
                }
            }
        }

        #region Create persistent objects

        private static void CreateOrder(UnitOfWork uow)
        {
            var order = new Order(uow);
            order.Date = DateTime.Now;
        }

        private static void CreateUser(UnitOfWork uow, string userName)
        {
            PermissionPolicyUser sampleUser = new XPQuery<PermissionPolicyUser>(uow).FirstOrDefault(x => x.UserName == userName);
            if (sampleUser != null)
            {
                sampleUser.Delete();
            }
            sampleUser = new PermissionPolicyUser(uow);
            sampleUser.UserName = userName;
            sampleUser.SetPassword("");
            PermissionPolicyRole defaultRole = userName == "User1" ? CreateDefaultRole1(uow) : CreateDefaultRole2(uow);
            sampleUser.Roles.Add(defaultRole);
        }

        private static PermissionPolicyRole CreateDefaultRole1(UnitOfWork uow)
        {
            PermissionPolicyRole defaultRole = new XPQuery<PermissionPolicyRole>(uow).FirstOrDefault(x => x.Name == "Default1");
            if (defaultRole != null)
            {
                defaultRole.Delete();
            }
            defaultRole = new PermissionPolicyRole(uow);
            defaultRole.Name = "Default1";
            defaultRole.AddTypePermissionsRecursively<Employee>(SecurityOperations.Read, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<Employee>(SecurityOperations.Write, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<Order>(SecurityOperations.Read, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<Customer>(SecurityOperations.Read, SecurityPermissionState.Allow);
            defaultRole.IsAdministrative = false;
            return defaultRole;
        }

        private static PermissionPolicyRole CreateDefaultRole2(UnitOfWork uow)
        {
            PermissionPolicyRole defaultRole = new XPQuery<PermissionPolicyRole>(uow).FirstOrDefault(x => x.Name == "Default2");
            if (defaultRole != null)
            {
                defaultRole.Delete();
            }
            defaultRole = new PermissionPolicyRole(uow);
            defaultRole.Name = "Default2";
            defaultRole.AddTypePermissionsRecursively<Employee>(SecurityOperations.Read, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<Employee>(SecurityOperations.Write, SecurityPermissionState.Allow);
            //defaultRole.AddTypePermissionsRecursively<Order>(SecurityOperations.Read, SecurityPermissionState.Allow);
            //defaultRole.AddTypePermissionsRecursively<Customer>(SecurityOperations.Read, SecurityPermissionState.Allow);
            defaultRole.IsAdministrative = false;
            return defaultRole;
        }

        #endregion

        #region Security        

        public static SecurityStrategyComplex GetSecurity(string userName)
        {
            AuthenticationStandard authentication = new AuthenticationStandard();
            SecurityStrategyComplex security = new SecurityStrategyComplex(typeof(PermissionPolicyUser), typeof(PermissionPolicyRole), authentication);
            //security.RegisterXPOAdapterProviders();            
            string password = string.Empty;
            authentication.SetLogonParameters(new AuthenticationStandardLogonParameters(userName, password));
            SecuritySystem.SetInstance(security);
            return security;
        }

        public static SecuredObjectSpaceProvider GetSecuredObjectSpaceProvider(SecurityStrategyComplex security)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SecuredObjectSpaceProvider objectSpaceProvider = new SecuredObjectSpaceProvider(security, connectionString, null);
            IObjectSpace loginObjectSpace = objectSpaceProvider.CreateObjectSpace();
            security.Logon(loginObjectSpace);
            return objectSpaceProvider;
        }

        #endregion

    }

}
