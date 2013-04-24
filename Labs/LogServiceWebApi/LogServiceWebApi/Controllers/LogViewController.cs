using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogServiceWebApi.Models;

namespace LogServiceWebApi.Controllers
{
    public class LogViewController : Controller
    {
        private ILogModelRepository db = new LogModelRepository();

        //
        // GET: /LogView/

        public ActionResult Index()
        {
            return View(db.All.ToList());
        }

        //
        // GET: /LogView/Details/5

        public ActionResult Details(Guid id)
        {
            LogModel logmodel = db.Find(id);
            if (logmodel == null)
            {
                return HttpNotFound();
            }
            return View(logmodel);
        }

        //
        // GET: /LogView/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /LogView/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LogModel logmodel)
        {
            if (ModelState.IsValid)
            {
                db.InsertOrUpdate(logmodel);
                return RedirectToAction("Index");
            }

            return View(logmodel);
        }

        //
        // GET: /LogView/Edit/5

        public ActionResult Edit(Guid id)
        {
            LogModel logmodel = db.Find(id);
            if (logmodel == null)
            {
                return HttpNotFound();
            }
            return View(logmodel);
        }

        //
        // POST: /LogView/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LogModel logmodel)
        {
            if (ModelState.IsValid)
            {
                db.InsertOrUpdate(logmodel);
                return RedirectToAction("Index");
            }
            return View(logmodel);
        }

        //
        // GET: /LogView/Delete/5

        public ActionResult Delete(Guid id)
        {
            LogModel logmodel = db.Find(id);
            if (logmodel == null)
            {
                return HttpNotFound();
            }
            return View(logmodel);
        }

        //
        // POST: /LogView/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            db.Delete(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}