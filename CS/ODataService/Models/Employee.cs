using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataService.Models
{
    [DefaultClassOptions]
    public class Employee : Person
    {
        public Employee(Session session) :
            base(session)
        {
        }
    }
}