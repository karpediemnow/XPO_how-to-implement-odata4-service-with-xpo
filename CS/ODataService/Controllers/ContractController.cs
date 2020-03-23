using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using ODataService.Controllers;

namespace WebApplication1.Controllers
{
    public class ContractController : BaseController
    {

        [EnableQuery]
        public IQueryable<Contract> Get()
        {
            return Session.Query<Contract>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<Contract> Get([FromODataUri] int key)
        {
            var result = Session.Query<Contract>().AsWrappedQuery().Where(t => t.ID == key);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public SingleResult<Customer> GetCustomer([FromODataUri] int key)
        {
            var result = Session.Query<Contract>().AsWrappedQuery().Where(m => m.ID == key).Select(m => m.Customer);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public SingleResult<BaseDocument> GetParentDocument([FromODataUri] int key)
        {
            var result = Session.Query<Contract>().AsWrappedQuery().Where(m => m.ID == key).Select(m => m.ParentDocument);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<BaseDocument> GetLinkedDocuments([FromODataUri] int key)
        {
            return Session.Query<Contract>().AsWrappedQuery().Where(m => m.ID == key).SelectMany(t => t.LinkedDocuments);
        }


        [HttpPost]
        public IHttpActionResult Post(Contract contract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            using (UnitOfWork uow = GetNewUow())
            {
                Contract entity = new Contract(uow)
                {
                    ID = contract.ID,
                    Date = contract.Date,
                    Number = contract.Number
                };
                uow.CommitChanges();
                return Created(entity);
            }
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] int key, Contract contract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (key != contract.ID)
            {
                return BadRequest();
            }
            using (UnitOfWork uow = GetNewUow())
            {
                Contract existing = uow.GetObjectByKey<Contract>(key);
                if (existing == null)
                {
                    Contract entity = new Contract(uow)
                    {
                        ID = contract.ID,
                        Date = contract.Date,
                        Number = contract.Number
                    };
                    uow.CommitChanges();
                    return Created(entity);
                }
                else
                {
                    existing.Date = contract.Date;
                    uow.CommitChanges();
                    return Updated(existing);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Contract> contract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Contract, int>(key, contract, this);
            if (result != null)
            {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            return StatusCode(ApiHelper.Delete<Contract, int>(key, this));
        }

        [HttpPost, HttpPut]
        public IHttpActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link)
        {
            return StatusCode(ApiHelper.CreateRef<Contract, int>(Request, key, navigationProperty, link, this));
        }

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty)
        {
            return StatusCode(ApiHelper.DeleteRef<Contract, int, int>(key, relatedKey, navigationProperty, this));
        }
    }
}