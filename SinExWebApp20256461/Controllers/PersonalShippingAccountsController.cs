﻿using System;
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
    public class PersonalShippingAccountsController : BaseController
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();


        // GET: PersonalShippingAccounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PersonalShippingAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ShippingAccountID,BuildingInformation,StreetInformation,City,ProvinceCode,PostalCode,CardType,CardNumber,SecurityNumber,CardHolderName,Month,Year,PhoneNumber,EmailAddress,FirstName,LastName")] PersonalShippingAccount personalShippingAccount)
        {
            if (ModelState.IsValid)
            {
                // db.ShippingAccounts.Add(personalShippingAccount);
                // db.SaveChanges();
                return RedirectToAction("Create", "Home");
            }

            return View(personalShippingAccount);
        }

        // GET: PersonalShippingAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (User.Identity.Name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var query =  db.PersonalShippingAccounts.FirstOrDefault(c => c.UserName == User.Identity.Name);
            id = query.ShippingAccountId;
            PersonalShippingAccount personalShippingAccount = (PersonalShippingAccount)db.ShippingAccounts.Find(id);
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            if (personalShippingAccount == null)
            {
                return HttpNotFound();
            }
            return View(personalShippingAccount);
        }
        // GET: PersonalShippingAccounts/Edit/5
        public ActionResult Details(int? id)
        {
            if (User.Identity.Name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var query = db.PersonalShippingAccounts.FirstOrDefault(c => c.UserName == User.Identity.Name);
            id = query.ShippingAccountId;
            PersonalShippingAccount personalShippingAccount = (PersonalShippingAccount)db.ShippingAccounts.Find(id);
            if (personalShippingAccount == null)
            {
                return HttpNotFound();
            }
            return View(personalShippingAccount);
        }
        // POST: PersonalShippingAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PersonalShippingAccount personalShippingAccount)
        {
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            if(!ValidateCard(personalShippingAccount.CardNumber, personalShippingAccount.CardType))
            {
                ViewBag.errorMessage = "the card number does not match the card type";
                return View(personalShippingAccount);
            }
            DateTime expireDate = new DateTime(int.Parse(personalShippingAccount.Year), int.Parse(personalShippingAccount.Month), 1);
            if (!isTodayOrLater(expireDate))
            {
                ViewBag.errorMessage = "expire date error";
                return View(personalShippingAccount);

            }
            if (ModelState.IsValid)
            {
                db.Entry(personalShippingAccount).State = EntityState.Modified;
                try
                {
                    if (!CityMatchProCode(personalShippingAccount.City, personalShippingAccount.ProvinceCode))
                    {
                        return View(personalShippingAccount);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return View();
                }
                return RedirectToAction("Index", "Home");
            }
            return View(personalShippingAccount);
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
