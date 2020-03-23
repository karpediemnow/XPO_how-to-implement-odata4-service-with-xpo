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
    public class OrderController : BaseController
    {

        [EnableQuery]
        public IQueryable<Order> Get()
        {
            return Session.Query<Order>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<Order> Get([FromODataUri] int key)
        {
            var result = Session.Query<Order>().AsWrappedQuery().Where(t => t.ID == key);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public SingleResult<Customer> GetCustomer([FromODataUri] int key)
        {
            var result = Session.Query<Order>().AsWrappedQuery().Where(m => m.ID == key).Select(m => m.Customer);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<OrderDetail> GetOrderDetails([FromODataUri] int key)
        {
            return Session.Query<OrderDetail>().AsWrappedQuery().Where(t => t.Order.ID == key);
        }

        [EnableQuery]
        public SingleResult<BaseDocument> GetParentDocument([FromODataUri] int key)
        {
            var result = Session.Query<Order>().AsWrappedQuery().Where(m => m.ID == key).Select(m => m.ParentDocument);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<BaseDocument> GetLinkedDocuments([FromODataUri] int key)
        {
            return Session.Query<Order>().AsWrappedQuery().Where(m => m.ID == key).SelectMany(t => t.LinkedDocuments);
        }

        [HttpPost]
        public IHttpActionResult Post(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            using (UnitOfWork uow = GetNewUow())
            {
                Order entity = new Order(uow)
                {
                    ID = order.ID,
                    Date = order.Date,
                    OrderStatus = order.OrderStatus
                };
                uow.CommitChanges();
                return Created(entity);
            }
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] int key, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (key != order.ID)
            {
                return BadRequest();
            }
            using (UnitOfWork uow = GetNewUow())
            {
                Order existing = uow.GetObjectByKey<Order>(key);
                if (existing == null)
                {
                    Order entity = new Order(uow)
                    {
                        ID = order.ID,
                        Date = order.Date,
                        OrderStatus = order.OrderStatus
                    };
                    uow.CommitChanges();
                    return Created(entity);
                }
                else
                {
                    existing.Date = order.Date;
                    uow.CommitChanges();
                    return Updated(existing);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Order> order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Order, int>(key, order, this);
            if (result != null)
            {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpPost]
        [HttpPut]
        [ODataRoute("Order({key})/OrderDetails")]
        public IHttpActionResult AddToOrderDetails([FromODataUri] int key, OrderDetail orderDetail)
        {
            using (UnitOfWork uow = GetNewUow())
            {
                Order order = uow.GetObjectByKey<Order>(key);
                if (order == null)
                {
                    return NotFound();
                }
                OrderDetail existing = order.OrderDetails.FirstOrDefault(d => d.OrderDetailID == orderDetail.OrderDetailID);
                if (existing == null)
                {
                    OrderDetail entity = new OrderDetail(uow)
                    {
                        Quantity = orderDetail.Quantity,
                        UnitPrice = orderDetail.UnitPrice,
                    };
                    order.OrderDetails.Add(entity);
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

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            return StatusCode(ApiHelper.Delete<Order, int>(key, this));
        }

        [HttpPost, HttpPut]
        public IHttpActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link)
        {
            return StatusCode(ApiHelper.CreateRef<Order, int>(Request, key, navigationProperty, link, this));
        }

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            return StatusCode(ApiHelper.DeleteRef<Order, int>(Request, key, navigationProperty, link, this));
        }

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty)
        {
            return StatusCode(ApiHelper.DeleteRef<Order, int, int>(key, relatedKey, navigationProperty, this));
        }

    }
}