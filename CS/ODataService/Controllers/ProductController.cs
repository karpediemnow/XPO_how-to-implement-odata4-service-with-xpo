using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using ODataService.Controllers;

namespace WebApplication1.Controllers
{
    public class ProductController : BaseController
    {

        [EnableQuery]
        public IQueryable<Product> Get()
        {
            return Session.Query<Product>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<Product> Get([FromODataUri] int key)
        {
            var result = Session.Query<Product>().AsWrappedQuery().Where(t => t.ProductID == key);
            return SingleResult.Create(result);
        }

        [HttpPost]
        public IHttpActionResult Post(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            using (UnitOfWork uow = GetNewUow())
            {
                Product entity = new Product(uow)
                {
                    ProductName = product.ProductName,
                    Picture = product.Picture
                };
                uow.CommitChanges();
                return Created(entity);
            }
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] int key, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (key != product.ProductID)
            {
                return BadRequest();
            }
            using (UnitOfWork uow = GetNewUow())
            {
                Product existing = uow.GetObjectByKey<Product>(key);
                if (existing == null)
                {
                    Product entity = new Product(uow)
                    {
                        ProductName = product.ProductName,
                        Picture = product.Picture
                    };
                    uow.CommitChanges();
                    return Created(entity);
                }
                else
                {
                    existing.ProductName = product.ProductName;
                    existing.Picture = product.Picture;
                    uow.CommitChanges();
                    return Updated(product);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Product> product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Product, int>(key, product, this);
            if (result != null)
            {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            return StatusCode(ApiHelper.Delete<Product, int>(key, this));
        }

        [HttpPost, HttpPut]
        public IHttpActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link)
        {
            return StatusCode(ApiHelper.CreateRef<Product, int>(Request, key, navigationProperty, link, this));
        }

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty)
        {
            return StatusCode(ApiHelper.DeleteRef<Product, int, int>(key, relatedKey, navigationProperty, this));
        }
    }
}