using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using ODataService.Controllers;

namespace WebApplication1.Controllers
{
    public class OrderDetailController : BaseController
    {

        [EnableQuery]
        public IQueryable<OrderDetail> Get()
        {
            return Session.Query<OrderDetail>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<OrderDetail> Get([FromODataUri] int key)
        {
            var result = Session.Query<OrderDetail>().AsWrappedQuery().Where(t => t.OrderDetailID == key);
            return SingleResult.Create(result);
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] int key, OrderDetail orderDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (key != orderDetail.OrderDetailID)
            {
                return BadRequest();
            }
            using (UnitOfWork uow = GetNewUow())
            {
                OrderDetail existing = uow.GetObjectByKey<OrderDetail>(key);
                if (existing == null)
                {
                    OrderDetail entity = new OrderDetail(uow)
                    {
                        Order = orderDetail.Order,
                        Quantity = orderDetail.Quantity,
                        UnitPrice = orderDetail.UnitPrice
                    };
                    uow.CommitChanges();
                    return Created(entity);
                }
                else
                {
                    existing.Quantity = orderDetail.Quantity;
                    existing.UnitPrice = orderDetail.UnitPrice;
                    uow.CommitChanges();
                    return Updated(existing);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<OrderDetail> orderDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = ApiHelper.Patch<OrderDetail, int>(key, orderDetail, this);
            if (result != null)
            {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            return StatusCode(ApiHelper.Delete<OrderDetail, int>(key, this));
        }
    }
}