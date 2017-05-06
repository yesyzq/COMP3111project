using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SinExWebApp20256461.Models;

namespace SinExWebApp20256461.Controllers
{
    public class PenaltyFeesController : Controller
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();

        // GET: PenaltyFees
        public ActionResult Index()
        {
            return View(db.PenaltyFees.ToList());
        }

        // GET: PenaltyFees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PenaltyFee penaltyFee = db.PenaltyFees.Find(id);
            if (penaltyFee == null)
            {
                return HttpNotFound();
            }
            return View(penaltyFee);
        }

        // GET: PenaltyFees/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PenaltyFees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PenaltyFeeID,Fee")] PenaltyFee penaltyFee)
        {
            if (ModelState.IsValid)
            {
                db.PenaltyFees.Add(penaltyFee);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return View();
                }
                return RedirectToAction("Index");
            }

            return View(penaltyFee);
        }

        // GET: PenaltyFees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PenaltyFee penaltyFee = db.PenaltyFees.Find(id);
            if (penaltyFee == null)
            {
                return HttpNotFound();
            }
            return View(penaltyFee);
        }

        // POST: PenaltyFees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PenaltyFeeID,Fee")] PenaltyFee penaltyFee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(penaltyFee).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return View();
                }
                return RedirectToAction("Index", "Home");
            }
            return View(penaltyFee);
        }

        // GET: PenaltyFees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PenaltyFee penaltyFee = db.PenaltyFees.Find(id);
            if (penaltyFee == null)
            {
                return HttpNotFound();
            }
            return View(penaltyFee);
        }

        // POST: PenaltyFees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PenaltyFee penaltyFee = db.PenaltyFees.Find(id);
            db.PenaltyFees.Remove(penaltyFee);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return View();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
