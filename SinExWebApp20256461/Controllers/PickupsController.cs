﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SinExWebApp20256461.Models;
using SinExWebApp20256461.ViewModels;
using System.Data.Entity.Validation;

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
        public ActionResult Create(int? waybillId, string pickupType, string location,string validDate, NewPickupViewModel pickupView = null)
        {
            pickupView = new NewPickupViewModel();
                 
            pickupView.Pickup = new Pickup
            {
                Date = DateTime.Now,
                Type = pickupType
            };
            ViewBag.WaybillId = waybillId;

            if (validDate == "false")
            {
                ViewBag.msg = "You can prearranged your pickup up to 5 days, please try another date";
                return View(pickupView);
            }

          
            /* bind shipment */
            var shipment = (from s in db.Shipments
                            where s.WaybillId == waybillId
                            select s).First();
            var buildingInfo = shipment.ShippingAccount.BuildingInformation;
            var streetInfo = shipment.ShippingAccount.StreetInformation;
            var cityInfo = shipment.ShippingAccount.City;
            var provinceCode = shipment.ShippingAccount.ProvinceCode;
            var postalCode = shipment.ShippingAccount.PostalCode;
            var senderMailingAddress = streetInfo + ", " + cityInfo + ", " + provinceCode + ", " + postalCode;
            if (buildingInfo != null)
            {
                senderMailingAddress = buildingInfo + ", " + senderMailingAddress;
            }

            /* bind savedAddress */
            var savedAddressNicknames = (from s in db.SavedAddresses
                                         where s.ShippingAccountId == shipment.ShippingAccountId && s.Type == "pickup"
                                         select s.NickName);
            if (savedAddressNicknames != null)
            {
                ViewBag.pickupLocations = savedAddressNicknames.Distinct().ToList();
            }
           
            if (location != null)
            {
                ViewBag.Location = location; //Same, Diff
                if (location == "Same")
                {
                    pickupView.Pickup.Location = senderMailingAddress;
                }

                if (location == "Diff")
                {
                    //search the address by the nickname
                    //TODO
                    pickupView.Pickup.Location = pickupView.PickupLocationNickname;
                }
            }
            return View(pickupView);
        }

        // POST: Pickups/Create
        [HttpPost]
      //  [ValidateAntiForgeryToken]    
        public ActionResult Create(string submit, NewPickupViewModel pickupView)   //model binding
        {
            
            DateTime endDate = DateTime.Now.AddDays(5);
            if (pickupView.Pickup.Date > endDate)
            {
                return RedirectToAction("Create", "Pickups", new { waybillId = pickupView.WaybillId, validDate = "false"});
            }


            /* Add saved address functionality */
            var shipment = (from s in db.Shipments
                            where s.WaybillId == pickupView.WaybillId
                            select s).First();
            var shippingAccount = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First();
            if (pickupView.RecipientNickname != null)
            {
                var r = shipment.Recipient;
                SavedAddress helper_address = new SavedAddress
                {
                    NickName = pickupView.RecipientNickname,
                    Street = r.Street,
                    City = r.City,
                    ProvinceCode = r.ProvinceCode,
                    PostalCode = r.PostalCode,
                    Type = "recipient",
                    ShippingAccountId = shippingAccount.ShippingAccountId
                };
                if (r.Building != null)
                {
                    helper_address.Building = r.Building;
                }
                shippingAccount.SavedAddresses.Add(helper_address);
                db.SavedAddresses.Add(helper_address);
            }
            if (pickupView.PickupNickname != null)
            {
                SavedAddress helper_address = new SavedAddress
                {
                    NickName = pickupView.PickupNickname,
                    PickupLocation = pickupView.Pickup.Location,
                    Type = "pickup",
                    ShippingAccountId = shippingAccount.ShippingAccountId
                };
                shippingAccount.SavedAddresses.Add(helper_address);
                db.SavedAddresses.Add(helper_address);
            }

            shipment.Pickup.Date = pickupView.Pickup.Date;
            shipment.Pickup.Location = pickupView.Pickup.Location;
            shipment.Pickup.Type = pickupView.Pickup.Type;

            /* need to add pickup */
          //  db.Pickups.Add(pickupView.Pickup);
            try
            {
                shipment.Status = "confirmed";
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Console.WriteLine(e);
            }
            return RedirectToAction("Index", "Shipments");
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
