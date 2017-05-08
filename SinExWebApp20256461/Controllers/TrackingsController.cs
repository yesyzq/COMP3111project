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
using System.Net.Mail;

namespace SinExWebApp20256461.Controllers
{
    public class TrackingsController : Controller
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();

        // GET: Trackings
        public ActionResult Index()
        {
            var a = 0;
            var trackings = db.Trackings;
            return View(trackings.ToList());
        }

        // GET: Trackings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tracking tracking = db.Trackings.Find(id);
            if (tracking == null)
            {
                return HttpNotFound();
            }
            return View(tracking);
        }

        // GET: Trackings/Create
        public ActionResult Create()
        {
            ViewBag.currTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            ViewBag.page = 1;
            // ViewBag.WaybillNumbers = new SelectList(db.Shipments.Select(a => a.WaybillNumber).Distinct());
            return View();
        }

        // POST: Trackings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TrackingID,WaybillNumber,DateTime,Description,Location,Remarks, Type, DeliveredTo, DeliveredAt")] Tracking tracking, string submit)
        {
            if (submit == "select waybill")
            {
                Shipment shipment = db.Shipments.FirstOrDefault(a => a.WaybillNumber == tracking.WaybillNumber);
                if (shipment == null)
                {
                    return Create();
                }

                string status = shipment.Status;
                if (status == "pending" || status == "delivered" || status == "lost" || status == "cancelled" || status == "returned")
                {
                    ViewBag.msg = "This shipment is not avaliable for tracking";
                    ViewBag.page = 1;
                    return View();
                }
                else if (status == "confirmed")
                {
                    ViewBag.Types = new string[1];
                    ViewBag.Types[0] = "picked_up";
                    ViewBag.page = 2;
                    return View(tracking);
                }
                else if (status == "picked_up" || status == "invoice_sent")
                {
                    ViewBag.Types = new string[4];
                    ViewBag.Types[0] = "delivered";
                    ViewBag.Types[1] = "lost";
                    ViewBag.Types[2] = "returned";
                    ViewBag.Types[3] = "other";
                    ViewBag.page = 2;
                    return View(tracking);
                }
            }
            else if (submit == "select type")
            {
                if (tracking.Type == "delivered")
                {
                    ViewBag.page = 3;
                }
                else
                {
                    ViewBag.page = 4;
                }
                return View(tracking);
            }
            else if (submit == "save")
            {
                if (ModelState.IsValid)
                {
                    string WaybillNumber = tracking.WaybillNumber;
                    tracking.WaybillId = db.Shipments.FirstOrDefault(a => a.WaybillNumber == WaybillNumber).WaybillId;
                    if (tracking.Type == "delivered" || tracking.Type == "lost" || tracking.Type == "picked_up" || tracking.Type == "returned")
                    {
                        Shipment shipment = db.Shipments.FirstOrDefault(a => a.WaybillNumber == WaybillNumber);
                        shipment.Status = tracking.Type;
                        
                        if(tracking.Type == "picked_up")
                        {
                            shipment.ShippedDate = tracking.DateTime;
                        }
                        if (tracking.Type == "delivered")
                        {
                            shipment.DeliveredDate = tracking.DateTime;
                        }
                        
                        // if send picked up notification to recipient
                        if (shipment.IfSendEmailRecipient == true && tracking.Type == "picked_up")
                        {
                            MailMessage mailMessage = new MailMessage();
                            //Add recipients
                            mailMessage.To.Add(shipment.Recipient.EmailAddress);
                            //mailMessage.To.Add(shipment.ShippingAccount.EmailAddress);
                            //Setting the displayed email address and display name
                            //!!!Do not use this to prank others!!!
                            mailMessage.From = new MailAddress("notification@sinex.com", "SinEx Notification");
                            var senderName = "";
                            if (shipment.ShippingAccount is PersonalShippingAccount)
                            {
                                PersonalShippingAccount person = (PersonalShippingAccount)shipment.ShippingAccount;
                                senderName = person.FirstName + " " + person.LastName;
                            }
                            else if (shipment.ShippingAccount is BusinessShippingAccount)
                            {
                                BusinessShippingAccount business = (BusinessShippingAccount)shipment.ShippingAccount;
                                senderName = business.ContactPersonName + ", " + business.CompanyName;
                            }
                            string senderAddr = shipment.ShippingAccount.BuildingInformation + ", "
                            + shipment.ShippingAccount.StreetInformation + ", "
                            + shipment.ShippingAccount.City + ", "
                            + shipment.ShippingAccount.ProvinceCode + ", "
                            + shipment.ShippingAccount.PostalCode;
                            //Subject and content of the email
                            mailMessage.Subject = "Pick up notification for Your Shipment (Waybill No. " + shipment.WaybillNumber + ")";
                            mailMessage.Body = "Dear Customer,\n \nYour shipment with waybillnumber " + shipment.WaybillNumber +
                                " has been picked up. \n\nDetailed information are as follows: \n"
                                + "Sender name:\t" + senderName
                                + "\nSender address:\t" + senderAddr
                                + "\nPick up date:\t" + tracking.DateTime;
                            mailMessage.Priority = MailPriority.Normal;

                            //Instantiate a new SmtpClient instance
                            SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");

                            //WARNING: DO NOT set any credentials and other settings!!!

                            //Send
                            try
                            {
                                smtpClient.Send(mailMessage);
                            }
                            catch (Exception e)
                            {
                                ViewBag.msg = e;
                                return View();
                            }

                        }

                        // if send delivered notification to sender
                        if (shipment.IfSendEmail == true && tracking.Type == "delivered")
                        {
                            MailMessage mailMessage = new MailMessage();
                            //Add recipients 
                            mailMessage.To.Add(shipment.ShippingAccount.EmailAddress);
                            //mailMessage.To.Add(shipment.Recipient.EmailAddress);
                            //Setting the displayed email address and display name
                            //!!!Do not use this to prank others!!!
                            mailMessage.From = new MailAddress("notification@sinex.com", "SinEx Notification");
                            var RecipientName = shipment.Recipient.FullName;
                            string RecipientAddr = shipment.Recipient.Building
                                + ", " + shipment.Recipient.Street
                                + ", " + shipment.Recipient.City
                                + ", " + shipment.Recipient.ProvinceCode;
                            //Subject and content of the email
                            mailMessage.Subject = "Delivery notification for Your Shipment (Waybill No. " + shipment.WaybillNumber + ")";
                            mailMessage.Body = "Dear Customer, \n \nYour shipment with waybillnumber " + shipment.WaybillNumber +
                                " has been delivered. \n\nDetailed information are as follows: \n"
                                + "Recipient name:\t" + RecipientName
                                + "\nRecipient address:\t" + RecipientAddr
                                + "\nDelivered date:\t" + tracking.DateTime;
                            mailMessage.Priority = MailPriority.Normal;

                            //Instantiate a new SmtpClient instance
                            SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");

                            //WARNING: DO NOT set any credentials and other settings!!!

                            //Send
                            try
                            {
                                smtpClient.Send(mailMessage);
                            }
                            catch (Exception e)
                            {
                                ViewBag.msg = e;
                                return View();
                            }
                        }
                    }
                    db.Trackings.Add(tracking);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        ViewBag.msg = e;
                        return View();
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            // ViewBag.WaybillId = new SelectList(db.Shipments, "WaybillId", "ReferenceNumber", tracking.WaybillId);
            return View(tracking);
        }

        // GET: Trackings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tracking tracking = db.Trackings.Find(id);
            if (tracking == null)
            {
                return HttpNotFound();
            }
            ViewBag.WaybillId = new SelectList(db.Shipments, "WaybillId", "ReferenceNumber", tracking.WaybillId);
            return View(tracking);
        }

        // POST: Trackings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TrackingID,WaybillId,DateTime,Description,Location,Remarks")] Tracking tracking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tracking).State = EntityState.Modified;
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
            ViewBag.WaybillId = new SelectList(db.Shipments, "WaybillId", "ReferenceNumber", tracking.WaybillId);
            return View(tracking);
        }

        // GET: Trackings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tracking tracking = db.Trackings.Find(id);
            if (tracking == null)
            {
                return HttpNotFound();
            }
            return View(tracking);
        }
        public ActionResult GetTracking(string WaybillNumber)
        {
            TrackingViewModel TrackingView = new TrackingViewModel();

            if (WaybillNumber == null)
            {
                return View(TrackingView);
            }
            TrackingView.Trackings = db.Trackings.Where(a => a.Shipment.WaybillNumber == WaybillNumber).OrderBy(a => a.DateTime).ToList();
            TrackingView.Shipment = db.Shipments.FirstOrDefault(a => a.WaybillNumber == WaybillNumber);

            var delivered_tracking = db.Trackings.FirstOrDefault(a => a.Shipment.WaybillNumber == WaybillNumber && a.Type == "delivered");
            if (delivered_tracking != null)
            {
                ViewBag.DeliveredTo = delivered_tracking.DeliveredTo ?? "";
                ViewBag.DeliveredAt = delivered_tracking.DeliveredAt ?? "";
            }
            else
            {
                ViewBag.DeliveredTo = "";
                ViewBag.DeliveredAt = "";
            }

            return View(TrackingView);
        }
        // POST: Trackings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tracking tracking = db.Trackings.Find(id);
            db.Trackings.Remove(tracking);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
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
