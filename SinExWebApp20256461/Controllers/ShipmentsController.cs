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
using X.PagedList;
using System.Windows.Forms;
using System.Data.Entity.Validation;

namespace SinExWebApp20256461.Controllers
{
    public class ShipmentsController : BaseController
    {  
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();
        // GET: Shipments/GenerateHistoryReport
        [Authorize(Roles = "Employee, Customer")]
        public ActionResult GenerateHistoryReport(string ShippingAccountNumber, string sortOrder, int? page, DateTime? ShippedStartDate, DateTime? ShippedEndDate)
        {
            // Instantiate an instance of the ShipmentsReportViewModel and the ShipmentsSearchViewModel.
            var shipmentSearch = new ShipmentsReportViewModel();
            shipmentSearch.Shipment = new ShipmentsSearchViewModel();
            ViewBag.CurrentSort = sortOrder;
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            if (string.IsNullOrEmpty(ShippingAccountNumber))
            {
                ShippingAccountNumber = "";
            }
            // Populate the ShippingAccountId dropdown list.
            shipmentSearch.Shipment.ShippingAccounts = PopulateShippingAccountsDropdownList().ToList();
            if (User.IsInRole("Customer"))
            {
                shipmentSearch.Shipment.ShippingAccounts = PopulateCustomerShippingAccountsDropdownList().ToList();
                //var currShippingAccount = db.Shipments.Where(a => a.ShippingAccount.UserName == User.Identity.Name).Select(a => a.ShippingAccountId).Distinct().ToList();
                //if (!currShippingAccount.Contains((int)ShippingAccountId))
                //{
                // shipmentSearch.Shipments = new ShipmentsListViewModel().ToPagedList;
                //return View(shipmentSearch);
                //}
            }
            ViewBag.CurrentShippingAccountNumber = ShippingAccountNumber;
            ViewBag.CurrentShippingStartDate = ShippedStartDate;
            ViewBag.CurrentShippingEndDate = ShippedEndDate;
            // Initialize the query to retrieve shipments using the ShipmentsListViewModel.
            var shipmentQuery = from s in db.Shipments
                                select new ShipmentsListViewModel
                                {
                                    WaybillId = s.WaybillId,
                                    ServiceType = s.ServiceType,
                                    ShippedDate = s.ShippedDate,
                                    DeliveredDate = s.DeliveredDate,
                                    RecipientName = s.RecipientName,
                                    NumberOfPackages = s.NumberOfPackages,
                                    Origin = s.Origin,
                                    Destination = s.Destination,
                                    ShippingAccountNumber = s.ShippingAccount.ShippingAccountNumber
                                };
            if (User.IsInRole("Customer"))
            {
                string userName = System.Web.HttpContext.Current.User.Identity.Name;
                shipmentQuery = from s in db.Shipments
                                where s.ShippingAccount.UserName == userName
                                select new ShipmentsListViewModel
                                {
                                    WaybillId = s.WaybillId,
                                    ServiceType = s.ServiceType,
                                    ShippedDate = s.ShippedDate,
                                    DeliveredDate = s.DeliveredDate,
                                    RecipientName = s.RecipientName,
                                    NumberOfPackages = s.NumberOfPackages,
                                    Origin = s.Origin,
                                    Destination = s.Destination,
                                    ShippingAccountNumber = s.ShippingAccount.ShippingAccountNumber
                                };
            }
            // Add the condition to select a spefic shipping account if shipping account id is not null.
            if (!string.IsNullOrEmpty(ShippingAccountNumber))
            {
                // TODO: Construct the LINQ query to retrive only the shipments for the specified shipping account id.
                shipmentQuery = from s in shipmentQuery
                                where s.ShippingAccountNumber == ShippingAccountNumber
                                select s;
                // shipmentSearch.Shipments = shipmentQuery.ToPagedList(pageNumber, pageSize);
            }
            else
            {
                // Return an empty result if no shipping account id has been selected.
                // shipmentSearch.Shipments = new ShipmentsListViewModel[0].ToPagedList(pageNumber, pageSize);
                shipmentQuery = from s in shipmentQuery
                                where s.ShippingAccountNumber == ""
                                select s;
                // page = 1;
            }

            if (ShippedStartDate != null && ShippedEndDate != null)
            {
                shipmentQuery = from s in shipmentQuery
                                where s.ShippedDate >= ShippedStartDate && s.ShippedDate <= ShippedEndDate
                                select s;
            }
            ViewBag.ServiceTypeSortParm = sortOrder == "ServiceType" ? "ServiceType_desc" : "ServiceType";
            ViewBag.ShippedDateSortParm = sortOrder == "ShippedDate" ? "ShippedDate_desc" : "ShippedDate";
            ViewBag.DeliveredDateSortParm = sortOrder == "DeliveredDate" ? "DeliveredDate_desc" : "DeliveredDate";
            ViewBag.RecipientNameSortParm = sortOrder == "RecipientName" ? "RecipientName_desc" : "RecipientName";
            ViewBag.OriginSortParm = sortOrder == "Origin" ? "Origin_desc" : "Origin";
            ViewBag.DestinationSortParm = sortOrder == "Destination" ? "Destination_desc" : "Destination";
            ViewBag.ShippingAccountIdSortParm = sortOrder == "ShippingAccountId" ? "ShippingAccountId_desc" : "ShippingAccountId";
            switch (sortOrder)
            {
                case "ServiceType":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.ServiceType);
                    break;
                case "ShippedDate":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.ShippedDate);
                    break;
                case "DeliveredDate":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.DeliveredDate);
                    break;
                case "RecipientName":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.RecipientName);
                    break;
                case "Origin":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.ServiceType);
                    break;
                case "Destination":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.Destination);
                    break;
                case "ShippingAccountId":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.ShippingAccountNumber);
                    break;
                case "ServiceType_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.ServiceType);
                    break;
                case "ShippedDate_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.ShippedDate);
                    break;
                case "DeliveredDate_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.DeliveredDate);
                    break;
                case "RecipientName_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.RecipientName);
                    break;
                case "Origin_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.ServiceType);
                    break;
                case "Destination_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.Destination);
                    break;
                case "ShippingAccountId_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.ShippingAccountNumber);
                    break;
                default:
                    shipmentQuery = shipmentQuery.OrderBy(s => s.WaybillId);
                    break;
            }
            shipmentSearch.Shipments = shipmentQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.msg = sortOrder;
            return View(shipmentSearch);
        }
        // this section of code is like shit
        public ActionResult getCost(string Origin, string Destination, string ServiceType, string PackageType, string Size, int? weights)
        {
            var cost = new CostViewModel();
            // cost.PackageTypes = (new SelectList(db.PackageTypes.Select(a => a.Type).Distinct())).ToList();
            // cost.ServiceTypes = (new SelectList(db.ServiceTypes.Select(a => a.Type).Distinct())).ToList();
            cost.PackageTypes = db.PackageTypes.Select(a => a.Type).Distinct().ToList();
            cost.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            cost.Origins = db.Destinations.Select(a => a.City).Distinct().ToList();
            cost.Destinations = db.Destinations.Select(a => a.City).Distinct().ToList();
            cost.Sizes = db.PakageTypeSizes.Select(a => a.size).Distinct().ToList();
            // int serviceTypeID = db.ServiceTypes.SingleOrDefault(s => s.Type == ServiceType).ServiceTypeID;
            // int packageTypeID = db.PackageTypes.SingleOrDefault(s => s.Type == PackageType).PackageTypeID;
            IEnumerable<ServicePackageFee> Fees = db.ServicePackageFees.Include(c => c.PackageType).Include(c => c.ServiceType);
            IEnumerable<PakageTypeSize> Sizes = db.PakageTypeSizes.Include(c => PackageType);
            double minimumFee = 0;
            double Fee = 0;
            foreach (var fee in Fees)
            {
                if (fee.ServiceType.Type == ServiceType && fee.PackageType.Type == PackageType)
                {
                    minimumFee = (double)fee.MinimumFee;
                    Fee = (double)fee.Fee;
                }
            }
            double HKDRate = db.Currencies.SingleOrDefault(s => s.CurrencyCode == "HKD").ExchangeRate;
            double MOPRate = db.Currencies.SingleOrDefault(s => s.CurrencyCode == "MOP").ExchangeRate;
            double TWDRate = db.Currencies.SingleOrDefault(s => s.CurrencyCode == "TWD").ExchangeRate;
            cost.Origin = Origin;
            cost.Destination = Destination;
            cost.ServiceType = ServiceType;
            cost.PackageType = PackageType;
            string weightLimit = "kg";
            foreach (var size in Sizes)
            {
                if (size.PackageType.Type == PackageType && size.size == Size)
                {
                    weightLimit = size.weightLimit;
                }
            }

            if (weights == null)
            {
                weights = 0;
            }
            cost.weights = (int)weights;
            if (ServiceType == null || PackageType == null)
            {
                cost.CNYcost = 0;
                cost.HKDcost = 0;
                cost.MOPcost = 0;
                cost.TWDcost = 0;
                return View(cost);
            }
            if (PackageType == "Envelope")
            {
                cost.CNYcost = Fee;
                cost.HKDcost = (double)ConvertCurrency("HKD", (decimal)Fee);
                cost.MOPcost = (double)ConvertCurrency("MOP", (decimal)Fee); ;
                cost.TWDcost = (double)ConvertCurrency("TWD", (decimal)Fee); ;
            }
            else if (PackageType == "Customer")
            {
                double price = (int)weights * Fee;
                if (price < minimumFee)
                {
                    price = minimumFee;
                }
                cost.CNYcost = price;
                cost.HKDcost = (double)ConvertCurrency("HKD", (decimal)price);
                cost.MOPcost = (double)ConvertCurrency("MOP", (decimal)price);
                cost.TWDcost = (double)ConvertCurrency("TWD", (decimal)price);
            }
            else
            {
                int weightLimitLength = weightLimit.Length - 2;
                if (weightLimitLength < 0)
                {
                    weightLimitLength = 0;
                }
                string weightSubString = weightLimit.Substring(0, weightLimitLength);
                int intWeightLimit = 0;
                if (weightSubString.Length > 0)
                {
                    if (weightLimit == "Not applicable")
                    {
                        intWeightLimit = 2147483647;
                    }
                    else
                    {
                        intWeightLimit = Int32.Parse(weightSubString);
                    }
                }
                else
                {
                    cost.CNYcost = 0;
                    cost.HKDcost = 0;
                    cost.MOPcost = 0;
                    cost.TWDcost = 0;
                    return View(cost);
                }
                double price = (int)weights * Fee;
                if (weights > intWeightLimit)
                {
                    price += 500;
                }
                if (price < minimumFee)
                {
                    price = minimumFee;
                }
                cost.CNYcost = price;
                cost.HKDcost = (double)ConvertCurrency("HKD", (decimal)price);
                cost.MOPcost = (double)ConvertCurrency("MOP", (decimal)price);
                cost.TWDcost = (double)ConvertCurrency("TWD", (decimal)price);
            }
            return View(cost);
        }
        private SelectList PopulateShippingAccountsDropdownList()
        {
            // TODO: Construct the LINQ query to retrieve the unique list of shipping account ids.
            var shippingAccountQuery = db.Shipments.Select(a => a.ShippingAccount.ShippingAccountNumber).Distinct().OrderBy(c => c);
            return new SelectList(shippingAccountQuery);
        }
        private SelectList PopulateCustomerShippingAccountsDropdownList()
        {
            // TODO: Construct the LINQ query to retrieve the unique list of shipping account ids.
            var shippingAccountQuery = db.Shipments.Where(a => a.ShippingAccount.UserName == User.Identity.Name).Select(a => a.ShippingAccountId).Distinct().OrderBy(c => c);
            return new SelectList(shippingAccountQuery);
        }
        // GET: Shipments
        public ActionResult Index()
        {
            var shippingAccount = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First();
            if (shippingAccount != null)
            {
                var ShippingAccountId = shippingAccount.ShippingAccountId;
                var shipments = from s in db.Shipments
                                where s.ShippingAccountId == ShippingAccountId
                                select s;
                return View(shipments.ToList());
            }
            return View();
        }

        // GET: Shipments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Shipment shipment = db.Shipments.Find(id);
            if (shipment == null)
            {
                return HttpNotFound();
            }
            return View(shipment);
        }

        // GET: Shipments/Create
        public ActionResult Create()
        {
            var shipmentView = new CreateShipmentViewModel();
            ViewBag.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(a => a.size).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();

            shipmentView.Packages = new List<Package>();
            var new_package = new Package();
            shipmentView.Packages.Add(new_package);
            return View(shipmentView);
        }

        // POST: Shipments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string ShipmentPayer, string TaxPayer, string submit, CreateShipmentViewModel shipmentView = null, Recipient recipient = null)
        {
            ViewBag.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(a => a.size).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();
            /* add packages */
            if (submit == "add")
            {
                if (shipmentView.Packages.Count < 10)
                {
                    var new_package = new Package();
                    shipmentView.Packages.Add(new_package);
                }
                return View(shipmentView);
            }
            else if (submit != null)
            {
                if(shipmentView.Packages.Count > 1)
                {
                    int id = Int32.Parse(submit.Split(' ')[1]) - 1;
                    shipmentView.Packages.Remove(shipmentView.Packages[id]);
                }
                return View(shipmentView);
            }
            if (ModelState.IsValid)
            {
                var shipment = shipmentView.Shipment;
                int num = db.Shipments.Count() + 1;
                string strNum = num.ToString().PadLeft(16, '0');
                shipment.WaybillNumber = strNum;
                shipment.Status = "pending";
                shipment.Recipient = recipient;
                shipment.ShippedDate = DateTime.Now;
                shipment.DeliveredDate = DateTime.Now;
                shipment.Packages = new List<Package>();
                for (int i = 0; i< shipmentView.Packages.Count; i++)
                {
                    shipmentView.Packages[i].WeightEstimated = Math.Round(shipmentView.Packages[i].WeightEstimated, 1);
                    shipment.Packages.Add(shipmentView.Packages[i]);
                    db.Packages.Add(shipmentView.Packages[i]);
                }
                shipment.NumberOfPackages = shipmentView.Packages.Count;

                /* bind shipping account */
                var shippingAccount = (from s in db.ShippingAccounts
                                       where s.UserName == User.Identity.Name
                                       select s).First();
                shipment.ShippingAccountId = shippingAccount.ShippingAccountId;

                /* create invoices */
                if (shipmentView.RecipientShippingAccountNumber != null)
                {
                    var recipientPayer = (from s in db.ShippingAccounts
                                          where s.ShippingAccountNumber == shipmentView.RecipientShippingAccountNumber
                                          select s).FirstOrDefault();
                    if (recipientPayer == null)
                    {
                        ModelState.AddModelError("recipientPayer", "recipient not found");
                        return View(shipmentView);
                    }
                }
                shipment.Invoices = new List<Invoice>();
                var shipmentInvoice = new Invoice { Type = "shipment" };
                shipmentInvoice.ShippingAccountNumber = ShipmentPayer == "Sender" ? shippingAccount.ShippingAccountNumber : shipmentView.RecipientShippingAccountNumber;
                var taxInvoice = new Invoice { Type = "tax_duty" };
                taxInvoice.ShippingAccountNumber = TaxPayer == "Sender" ? shippingAccount.ShippingAccountNumber : shipmentView.RecipientShippingAccountNumber;
                shipment.Invoices.Add(shipmentInvoice);
                shipment.Invoices.Add(taxInvoice);
                          
                /* Add shipping account helper address 
                if (shipmentView.Nickname != null)
                {
                    var r = shipmentView.Recipient;
                    SavedAddress helper_address = new SavedAddress
                    {
                        NickName = shipmentView.Nickname,
                        //Address = r.Street + ":" + r.City + ":" + r.ProvinceCode,
                        Type = "recipient"
                    };
                    if (r.Building != null)
                    {
                        //helper_address.Address = r.Building + ":" + helper_address.Address;
                    }
                    shippingAccount.SavedAddresses.Add(helper_address);
                    db.SavedAddresses.Add(helper_address);
                } */

                shipment.IfSendEmail = shipmentView.IfSendEmail == "Yes" ? true : false;

                var pickup = new Pickup();
                pickup.Date = DateTime.Now;
                shipment.Pickup = pickup;
                
                db.Invoices.Add(taxInvoice);
                db.Invoices.Add(shipmentInvoice);
                db.Recipients.Add(recipient);
                
                db.Shipments.Add(shipment);
                try
                {
                    db.SaveChanges();
                }
                catch(DbEntityValidationException e)
                {
                    Console.WriteLine(e);
                }
                

                ViewBag.waybillId = shipment.WaybillId;

                return RedirectToAction("Index");
            }

            return View(shipmentView);
        }

        // GET: Shipments/Edit
        public ActionResult Edit(int? id)
        {
            Shipment shipment = db.Shipments.Find(id);
            if (shipment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ret = new CreateShipmentViewModel {
                Shipment = shipment,
                Packages = shipment.Packages.ToList()
             };
            ViewBag.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(a => a.size).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();

            return View(ret);
        }

        // POST: Shipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, CreateShipmentViewModel shipmentView, string submit, string IfSendEmail, string ShipmentPayer, string TaxPayer)
        {   
            if (ModelState.IsValid)
            {
                ViewBag.PackageCurrency = db.Currencies.Select(m => m.CurrencyCode).Distinct().ToList();
                ViewBag.ServiceTypes = db.ServiceTypes.Select(m => m.Type).Distinct().ToList();
                ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(m => m.size).Distinct().ToList();

                var shipment = shipmentView.Shipment;
                var shipmentDB = db.Shipments.Find(id);
              
                if (submit == "add" && shipmentView.Packages.Count < 10)
                {
                    var new_package = new Package();
                    shipmentView.Packages.Add(new_package);
                    return View(shipmentView);
                }

                if (IfSendEmail == "Yes")
                {
                    shipment.IfSendEmail = true;
                }
                else
                {
                    shipment.IfSendEmail = false;
                }

                /* Update Invoice */
                int i_id1 = shipmentDB.Invoices.ToList()[0].InvoiceID;
                int i_id2 = shipmentDB.Invoices.ToList()[1].InvoiceID;
                var _invoice1 = db.Invoices.Find(i_id1);
                var _invoice2 = db.Invoices.Find(i_id2);
                var a = shipmentDB.ShippingAccount.ShippingAccountNumber;
                _invoice1.ShippingAccountNumber = ShipmentPayer == "Sender" ? a : shipmentView.RecipientShippingAccountNumber;
                _invoice2.ShippingAccountNumber = TaxPayer == "Sender" ? a : shipmentView.RecipientShippingAccountNumber;

                /* Update recipient */
                int r_id = shipmentDB.Recipient.RecipientID;
                var _recipient = db.Recipients.Find(r_id);
                _recipient.FullName = shipment.Recipient.FullName;
                _recipient.Building = shipment.Recipient.Building;
                _recipient.Street = shipment.Recipient.Street;
                _recipient.City = shipment.Recipient.City;
                _recipient.ProvinceCode = shipment.Recipient.ProvinceCode;
                _recipient.PostalCode = shipment.Recipient.PostalCode;
                _recipient.PhoneNumber = shipment.Recipient.PhoneNumber;
                _recipient.EmailAddress = shipment.Recipient.EmailAddress;

                /* Update packages */
                var old_packages = from s in db.Packages
                                   where s.WaybillId == shipmentDB.WaybillId
                                   select s;
                foreach (var i in old_packages)
                    db.Packages.Remove(i);
                for (int i = 0; i < shipmentView.Packages.Count; i++)
                {
                    shipmentDB.Packages.Add(shipmentView.Packages[i]);
                    db.Packages.Add(shipmentView.Packages[i]);
                }
                shipmentDB.NumberOfPackages = shipmentView.Packages.Count;

                /* Update shipment*/
                shipmentDB.ReferenceNumber = shipment.ReferenceNumber;
                shipmentDB.Origin = shipment.Origin;
                shipmentDB.Destination = shipment.Destination;
                shipmentDB.ServiceType = shipment.ServiceType;
                shipmentDB.IfSendEmail = IfSendEmail == "Yes" ? true : false;

                db.Entry(_invoice1).State = EntityState.Modified;
                db.Entry(_invoice2).State = EntityState.Modified;
                db.Entry(_recipient).State = EntityState.Modified;
                db.Entry(shipmentDB).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            
            return RedirectToAction("Index");
        }

        // GET: Shipments/Delete/5
        public ActionResult Delete(int? id)
        {
            Shipment shipment = db.Shipments.Find(id);
            if (shipment == null)
            {
                return HttpNotFound();
            }
            
            return View(shipment);
        }

        // POST: Shipments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Shipment shipment = db.Shipments.Find(id);
            //db.Shipments.Remove(shipment);
            shipment.Status = "deleted";
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
