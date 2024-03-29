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
    public class BusinessShippingAccountsController : BaseController
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();

        // GET: BusinessShippingAccounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BusinessShippingAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ShippingAccountID,BuildingInformation,StreetInformation,City,ProvinceCode,PostalCode,CardType,CardNumber,SecurityNumber,CardHolderName,Month,Year,PhoneNumber,EmailAddress,ContactPersonName,CompanyName,DepartmentName")] BusinessShippingAccount businessShippingAccount)
        {
            if (ModelState.IsValid)
            {
                db.ShippingAccounts.Add(businessShippingAccount);
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

            return View(businessShippingAccount);
        }

        // GET: BusinessShippingAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            if (User.Identity.Name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var query = db.BusinessShippingAccounts.FirstOrDefault(c => c.UserName == User.Identity.Name);
            id = query.ShippingAccountId;
            BusinessShippingAccount businessShippingAccount = (BusinessShippingAccount)db.ShippingAccounts.Find(id);
            if (businessShippingAccount == null)
            {
                return HttpNotFound();
            }
            return View(businessShippingAccount);
        }
        // GET: BusinessShippingAccounts/Details/5
        public ActionResult Details(int? id)
        {
            if (User.Identity.Name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var query = db.BusinessShippingAccounts.FirstOrDefault(c => c.UserName == User.Identity.Name);
            id = query.ShippingAccountId;
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            BusinessShippingAccount businessShippingAccount = (BusinessShippingAccount)db.ShippingAccounts.Find(id);
            if (businessShippingAccount == null)
            {
                return HttpNotFound();
            }
            return View(businessShippingAccount);
        }
        // POST: BusinessShippingAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ShippingAccountNumber, UserName, ShippingAccountID,BuildingInformation,StreetInformation,City,ProvinceCode,PostalCode,CardType,CardNumber,SecurityNumber,CardHolderName,Month,Year,PhoneNumber,EmailAddress,ContactPersonName,CompanyName,DepartmentName")] BusinessShippingAccount businessShippingAccount)
        {
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            if (!ValidateCard(businessShippingAccount.CardNumber, businessShippingAccount.CardType))
            {
                ViewBag.errorMessage = "the card number does not match the card type";
                return View(businessShippingAccount);
            }
            DateTime expireDate = new DateTime(int.Parse(businessShippingAccount.Year), int.Parse(businessShippingAccount.Month), 1);
            if (!isTodayOrLater(expireDate))
            {
                ViewBag.errorMessage = "expire date error";
                return View(businessShippingAccount);
            }
            if (ModelState.IsValid)
            {
                db.Entry(businessShippingAccount).State = EntityState.Modified;
                try
                {
                    if (!CityMatchProCode(businessShippingAccount.City, businessShippingAccount.ProvinceCode))
                    {
                        return View(businessShippingAccount);
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
            return View(businessShippingAccount);
        }
    }
}
