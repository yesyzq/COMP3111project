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
using System.Data.Entity.Validation;

namespace SinExWebApp20256461.Controllers
{


    public class SavedAddressesController : Controller
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();

        // GET: SavedAddresses
        public ActionResult Index(string waybillId)
        {
            ViewBag.WaybillId = waybillId;
            var shippingAccount = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First();
            if (shippingAccount != null)
            {
                var ShippingAccountId = shippingAccount.ShippingAccountId;
                var addresses = from s in db.SavedAddresses
                                where s.ShippingAccountId == ShippingAccountId
                                select s;
                return View(addresses.ToList());
            }
            return View();
        }

        // GET: SavedAddresses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedAddress savedAddress = db.SavedAddresses.Find(id);
            if (savedAddress == null)
            {
                return HttpNotFound();
            }
            return View(savedAddress);
        }

        // GET: SavedAddresses/Create
        public ActionResult Create(string type, string waybillId)
        {
            SavedAddressViewModel viewModel = new SavedAddressViewModel();
            viewModel.SavedAddress = new SavedAddress
            {
                Type=type
            };

            viewModel.WaybillId = waybillId;
            viewModel.PageJumpType = type;//for jumping from pickup to savedAddress
            ViewBag.WaybillId = waybillId;
            ViewBag.ShippingAccountId = new SelectList(db.ShippingAccounts, "ShippingAccountId", "ShippingAccountNumber");
            return View(viewModel);
        }

        // POST: SavedAddresses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string type_post,SavedAddressViewModel savedAddressViewModel)
        {
            SavedAddress savedAddress = new SavedAddress();
            savedAddress = savedAddressViewModel.SavedAddress;

            /*Empty entry exception */
            if(savedAddressViewModel.SavedAddress.NickName == null)
            {
                ViewBag.errorMessage = "You are required to assign a nickname to the location";
                return View(savedAddressViewModel);
            }


            ViewBag.ShippingAccountId = new SelectList(db.ShippingAccounts, "ShippingAccountId", "ShippingAccountNumber", savedAddress.ShippingAccountId);
            var shippingAccount = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First();
            bool isExist = (from s in db.SavedAddresses
                            where s.ShippingAccountId == shippingAccount.ShippingAccountId
                            && s.NickName == savedAddress.NickName
                            select s).Any();
            if (isExist)
            {
                ViewBag.errorMessage = "The nickname already exists! Please choose another one";
                return View(savedAddressViewModel);
            }
            savedAddress.ShippingAccountId = shippingAccount.ShippingAccountId;
            db.SavedAddresses.Add(savedAddress);

            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Console.WriteLine(e);
            }
            

            if(type_post == "CreateAndReturnToPickup")
            {
                return RedirectToAction("Create","Pickups",new { waybillId =savedAddressViewModel.WaybillId});
            }

            return RedirectToAction("Index");
        }

        // GET: SavedAddresses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedAddress savedAddress = db.SavedAddresses.Find(id);
            if (savedAddress == null)
            {
                return HttpNotFound();
            }
            ViewBag.ShippingAccountId = new SelectList(db.ShippingAccounts, "ShippingAccountId", "ShippingAccountNumber", savedAddress.ShippingAccountId);
            return View(savedAddress);
        }

        // POST: SavedAddresses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SavedAddress savedAddress)
        {
            var shippingAccount = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First();
            savedAddress.ShippingAccountId = shippingAccount.ShippingAccountId;

            if (ModelState.IsValid)
            {
                db.Entry(savedAddress).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    Console.WriteLine(e);
                }
                return RedirectToAction("Index");
            }
            ViewBag.ShippingAccountId = new SelectList(db.ShippingAccounts, "ShippingAccountId", "ShippingAccountNumber", savedAddress.ShippingAccountId);
            return View(savedAddress);
        }

        // GET: SavedAddresses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedAddress savedAddress = db.SavedAddresses.Find(id);
            if (savedAddress == null)
            {
                return HttpNotFound();
            }
            return View(savedAddress);
        }

        // POST: SavedAddresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SavedAddress savedAddress = db.SavedAddresses.Find(id);
            db.SavedAddresses.Remove(savedAddress);
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Console.WriteLine(e);
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
