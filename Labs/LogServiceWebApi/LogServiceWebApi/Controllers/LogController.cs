using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using LogServiceWebApi.Models;

namespace LogServiceWebApi.Controllers
{
    public class LogController : ApiController
    {
        private ILogModelRepository db = new LogModelRepository();

        // GET api/Log
        public IEnumerable<LogModel> GetLogModels()
        {
            return db.All;
        }

        // GET api/Log/5
        public LogModel GetLogModel(Guid id)
        {
            LogModel logmodel = db.Find(id);
            if (logmodel == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return logmodel;
        }

        // PUT api/Log/5
        public HttpResponseMessage PutLogModel(Guid id, LogModel logmodel)
        {
            try
            {
                db.InsertOrUpdate(logmodel);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/Log
        public HttpResponseMessage PostLogModel(LogModel logmodel)
        {
            if (ModelState.IsValid)
            {
                db.InsertOrUpdate(logmodel);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, logmodel);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = logmodel.ID }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Log/5
        public HttpResponseMessage DeleteLogModel(Guid id)
        {
            LogModel logmodel = db.Find(id);
            if (logmodel == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            try
            {
                db.Delete(id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, logmodel);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}