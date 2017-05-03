using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SinExWebApp20256461.Models;
using SinExWebApp20256461.ViewModels;

namespace SinExWebApp20256461.Controllers
{
    public class PickupsController : Controller
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();

        public object Viewbag { get; private set; }

        public ActionResult New(bool? isSameAsSender, string type)
        {
            List<string> pickupTypes = new List<string>();
            pickupTypes.Add("Immediate");
            pickupTypes.Add("Prearranged");

            ViewBag.IsSameAsSender = isSameAsSender;
            ViewBag.Type = type;

            var viewModel = new NewPickupViewModel();
            return View(viewModel);
        }


        // GET: Pickups
        public ActionResult Index()
        {
            return View(db.Pickups.ToList());
        }

        // GET: Pickups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pickup pickup = db.Pickups.Find(id);
            if (pickup == null)
            {
                return HttpNotFound();
            }
            return View(pickup);
        }

        // GET: Pickups/Create
        public ActionResult Create(int? waybillId, string pickupType, string location, NewPickupViewModel pickupView = null)   //model binding
        {
            pickupView = new NewPickupViewModel();
            pickupView.Pickup = new Pickup();
            pickupView.Pickup.Date = DateTime.Now;
            pickupView.Pickup.Type = pickupType;
            ViewBag.WaybillId=waybillId;

            /* bind shipment */
            var shipment = (from s in db.Shipments
                            where s.WaybillId == waybillId
                            select s).First();
            var buildingInfo = shipment.ShippingAccount.BuildingInformation;
            var streetInfo = shipment.ShippingAccount.StreetInformation;
            var cityInfo = shipment.ShippingAccount.City;
            var provinceCode = shipment.ShippingAccount.ProvinceCode;
            var postalCode = shipment.ShippingAccount.PostalCode;
            var senderMailingAddress = buildingInfo + ":" + streetInfo + ":" + cityInfo + ":" + provinceCode + ":" + postalCode;

            if (location != null)
            {
                ViewBag.Location = location;//Same, Diff
                if(location == "Same")
                {
                    pickupView.Pickup.Location = senderMailingAddress;
                }

                else if (location == "Diff")
                {
                    // ViewBag.pickupLocations = shipment.ShippingAccount.SavedAddresses.Select(a => a.Address).ToList();
                    //hardcode test
                    var locations = new List<string>();
                    locations.Add("Home:123,UG Hall 3,HKUST");
                    locations.Add("Office: 4213, CSE lab, HKUST");
                    ViewBag.pickupLocations = locations;
                }



                return View(pickupView);
            }

            return View(pickupView);
        }


            // POST: Pickups/Create
            // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
            // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
 //       [ValidateAntiForgeryToken]    
        public ActionResult Create(string submit,NewPickupViewModel pickupView = null)   //model binding
        {
            pickupView = new NewPickupViewModel();
            pickupView.Pickup = new Pickup();
            pickupView.Pickup.Date = DateTime.Now;
            
           

            return View(pickupView);
            /*
            db.Pickups.Add(pickup);
            db.SaveChanges();


            return RedirectToAction("Index", "Shipments");

    */
        }

        // GET: Pickups/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pickup pickup = db.Pickups.Find(id);
            if (pickup == null)
            {
                return HttpNotFound();
            }
            return View(pickup);
        }

        // POST: Pickups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PickupID,Location,Date,Type")] Pickup pickup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pickup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(pickup);
        }

        // GET: Pickups/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pickup pickup = db.Pickups.Find(id);
            if (pickup == null)
            {
                return HttpNotFound();
            }
            return View(pickup);
        }

        // POST: Pickups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pickup pickup = db.Pickups.Find(id);
            db.Pickups.Remove(pickup);
            db.SaveChanges();
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
