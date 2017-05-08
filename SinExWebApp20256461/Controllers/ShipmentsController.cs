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
using Invoicer.Helpers;
using Invoicer.Models;
using Invoicer.Services.Impl;
using Invoicer.Services;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Net.Mail;

namespace SinExWebApp20256461.Controllers
{
    public class ShipmentsController : BaseController
    {
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();
        // GET: Shipments/GenerateHistoryReport
        [Authorize(Roles = "Employee, Customer")]
        public ActionResult GenerateHistoryReport(string ShippingAccountNumber, string sortOrder, int? page, string ShippedStartDate, string ShippedEndDate, int? FirstClick)
        {
            // Instantiate an instance of the ShipmentsReportViewModel and the ShipmentsSearchViewModel.
            var shipmentSearch = new ShipmentsReportViewModel();
            shipmentSearch.Shipment = new ShipmentsSearchViewModel();
            int pageSize = 5;
            int pageNumber = (page ?? 1);

            shipmentSearch.Shipment.ShippingAccounts = PopulateShippingAccountsDropdownList().ToList();
            
            if (FirstClick == 1)
            {
                ShippedStartDate = DateTime.Today.ToString("yyyy-MM-dd");
                ShippedEndDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            shipmentSearch.Shipment.ShippedStartDate = ShippedStartDate;
            shipmentSearch.Shipment.ShippedEndDate= ShippedEndDate;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentShippingAccountNumber = ShippingAccountNumber;
            ViewBag.CurrentShippedStartDate = ShippedStartDate;
            ViewBag.CurrentShippedEndDate = ShippedEndDate;

            IQueryable<ShipmentsListViewModel> shipmentQuery;

            // Initialize the query to retrieve shipments using the ShipmentsListViewModel.
            if (User.IsInRole("Employee"))
            {
                shipmentQuery = from s in db.Shipments
                                where s.ShippingAccount.ShippingAccountNumber == ShippingAccountNumber
                                select new ShipmentsListViewModel
                                {
                                    WaybillNumber = db.Shipments.FirstOrDefault(a => a.WaybillId == s.WaybillId).WaybillNumber,
                                    ServiceType = s.ServiceType,
                                    ShippedDate = s.ShippedDate,
                                    DeliveredDate = s.DeliveredDate,
                                    RecipientName = s.Recipient.FullName,
                                    NumberOfPackages = s.NumberOfPackages,
                                    Origin = s.Origin,
                                    Destination = s.Destination,
                                    ShippingAccountNumber = s.ShippingAccount.ShippingAccountNumber,
                                    Status = s.Status
                                };
            }
            else //if (User.IsInRole("Customer"))
            {
                string userName = System.Web.HttpContext.Current.User.Identity.Name;

                shipmentQuery = from s in db.Shipments
                                where s.ShippingAccount.UserName == userName
                                select new ShipmentsListViewModel
                                {
                                    WaybillNumber = db.Shipments.FirstOrDefault(a => a.WaybillId == s.WaybillId).WaybillNumber,
                                    ServiceType = s.ServiceType,
                                    ShippedDate = s.ShippedDate,
                                    DeliveredDate = s.DeliveredDate,
                                    RecipientName = s.Recipient.FullName,
                                    NumberOfPackages = s.NumberOfPackages,
                                    Origin = s.Origin,
                                    Destination = s.Destination,
                                    ShippingAccountNumber = s.ShippingAccount.ShippingAccountNumber,
                                    Status = s.Status
                                };
            }

            if (!string.IsNullOrWhiteSpace(ShippedStartDate))
            {
                DateTime shippedStartDate = DateTime.ParseExact(ShippedStartDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                shippedStartDate = shippedStartDate.AddDays(-1);
                shipmentQuery = shipmentQuery.Where(a => a.ShippedDate > shippedStartDate);
            }

            if (!string.IsNullOrWhiteSpace(ShippedEndDate))
            {
                DateTime shippedEndDate = DateTime.ParseExact(ShippedEndDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                shippedEndDate = shippedEndDate.AddDays(1);
                shipmentQuery = shipmentQuery.Where(a => a.ShippedDate < shippedEndDate);
            }

            ViewBag.ServiceTypeSortParm = sortOrder == "ServiceType" ? "ServiceType_desc" : "ServiceType";
            ViewBag.ShippedDateSortParm = sortOrder == "ShippedDate" ? "ShippedDate_desc" : "ShippedDate";
            ViewBag.DeliveredDateSortParm = sortOrder == "DeliveredDate" ? "DeliveredDate_desc" : "DeliveredDate";
            ViewBag.RecipientNameSortParm = sortOrder == "RecipientName" ? "RecipientName_desc" : "RecipientName";
            ViewBag.OriginSortParm = sortOrder == "Origin" ? "Origin_desc" : "Origin";
            ViewBag.DestinationSortParm = sortOrder == "Destination" ? "Destination_desc" : "Destination";
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
                    shipmentQuery = shipmentQuery.OrderBy(s => s.Origin);
                    break;
                case "Destination":
                    shipmentQuery = shipmentQuery.OrderBy(s => s.Destination);
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
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.Origin);
                    break;
                case "Destination_desc":
                    shipmentQuery = shipmentQuery.OrderByDescending(s => s.Destination);
                    break;
                default:
                    shipmentQuery = shipmentQuery.OrderBy(s => s.WaybillNumber);
                    break;
            }
            shipmentSearch.Shipments = shipmentQuery.ToPagedList(pageNumber, pageSize);
            ViewBag.msg = sortOrder;
            return View(shipmentSearch);
        }

        [Authorize(Roles = "Employee, Customer")]
        public ActionResult GenerateInvoiceReport(string ShippingAccountNumber, string sortOrder, int? page, string ShippedStartDate, string ShippedEndDate, int? FirstClick)
        {
            // Instantiate an instance of the ShipmentsReportViewModel and the ShipmentsSearchViewModel.
            var invoiceSearch = new InvoicesReportViewModel();
            invoiceSearch.Invoice = new InvoicesSearchViewModel();
            int pageSize = 5;
            int pageNumber = (page ?? 1);

            // Populate the ShippingAccountNumber dropdown list.
            invoiceSearch.Invoice.ShippingAccounts = new SelectList(db.Invoices.Select(a => a.ShippingAccountNumber).Distinct().OrderBy(c => c)).ToList();

            if (FirstClick == 1)
            {
                ShippedStartDate = DateTime.Today.ToString("yyyy-MM-dd");
                ShippedEndDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            invoiceSearch.Invoice.ShippedStartDate = ShippedStartDate;
            invoiceSearch.Invoice.ShippedEndDate = ShippedEndDate;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentShippingAccountNumber = ShippingAccountNumber;
            ViewBag.CurrentShippedStartDate = ShippedStartDate;
            ViewBag.CurrentShippedEndDate = ShippedEndDate;

            IQueryable<InvoicesListViewModel> invoiceQuery;

            // Initialize the query to retrieve shipments using the ShipmentsListViewModel.
            string[] valid_statuses = { "invoice_sent", "delivered", "lost", "cancelled" };
            if (User.IsInRole("Employee"))
            {
                invoiceQuery = from s in db.Invoices
                                where s.ShippingAccountNumber == ShippingAccountNumber && valid_statuses.Contains(s.Shipment.Status)
                                select new InvoicesListViewModel
                                {
                                    WaybillNumber = db.Shipments.FirstOrDefault(a => a.WaybillId == s.WaybillId).WaybillNumber,
                                    ServiceType = s.Shipment.ServiceType,
                                    ShippedDate = s.Shipment.ShippedDate,
                                    RecipientName = s.Shipment.Recipient.FullName,
                                    TotalAmountPayable = s.TotalAmountPayable,
                                    Origin = s.Shipment.Origin,
                                    Destination = s.Shipment.Destination,
                                    ShippingAccountNumber = s.ShippingAccountNumber,
                                    Type = s.Type,
                                    Status = s.Shipment.Status
                                };
            }
            else //if (User.IsInRole("Customer"))
            {
                string userName = System.Web.HttpContext.Current.User.Identity.Name;
                invoiceQuery = from s in db.Invoices
                               where s.Shipment.ShippingAccount.UserName == userName && valid_statuses.Contains(s.Shipment.Status)
                               select new InvoicesListViewModel
                               {
                                   WaybillNumber = db.Shipments.FirstOrDefault(a => a.WaybillId == s.WaybillId).WaybillNumber,
                                   ServiceType = s.Shipment.ServiceType,
                                   ShippedDate = s.Shipment.ShippedDate,
                                   RecipientName = s.Shipment.Recipient.FullName,
                                   TotalAmountPayable = s.TotalAmountPayable,
                                   Origin = s.Shipment.Origin,
                                   Destination = s.Shipment.Destination,
                                   ShippingAccountNumber = s.ShippingAccountNumber,
                                   Type = s.Type,
                                   Status = s.Shipment.Status
                               };
            }

            if (!string.IsNullOrWhiteSpace(ShippedStartDate))
            {
                DateTime shippedStartDate = DateTime.ParseExact(ShippedStartDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                shippedStartDate = shippedStartDate.AddDays(-1);
                invoiceQuery = invoiceQuery.Where(a => a.ShippedDate > shippedStartDate);
            }

            if (!string.IsNullOrWhiteSpace(ShippedEndDate))
            {
                DateTime shippedEndDate = DateTime.ParseExact(ShippedEndDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                shippedEndDate = shippedEndDate.AddDays(1);
                invoiceQuery = invoiceQuery.Where(a => a.ShippedDate < shippedEndDate);
            }

            ViewBag.ServiceTypeSortParm = (sortOrder == "serviceType") ? "serviceType_desc" : "serviceType";
            ViewBag.ShippedDateSortParm = (sortOrder == "shippedDate") ? "shippedDate_desc" : "shippedDate";
            ViewBag.RecipientNameSortParm = (sortOrder == "recipientName") ? "recipientName_desc" : "recipientName";
            ViewBag.OriginSortParm = (sortOrder == "origin") ? "origin_desc" : "origin";
            ViewBag.DestinationSortParm = (sortOrder == "destination") ? "destination_desc" : "destination";
            ViewBag.ToalInvoiceAmountSortParm = (sortOrder == "toalInvoiceAmount") ? "toalInvoiceAmount_desc" : "toalInvoiceAmount";

            switch (sortOrder)
            {
                case "serviceType":
                    invoiceQuery = invoiceQuery.OrderBy(s => s.ServiceType);
                    break;
                case "serviceType_desc":
                    invoiceQuery = invoiceQuery.OrderByDescending(s => s.ServiceType);
                    break;
                case "shippedDate":
                    invoiceQuery = invoiceQuery.OrderBy(s => s.ShippedDate);
                    break;
                case "shippedDate_desc":
                    invoiceQuery = invoiceQuery.OrderByDescending(s => s.ShippedDate);
                    break;
                case "recipientName":
                    invoiceQuery = invoiceQuery.OrderBy(s => s.RecipientName);
                    break;
                case "recipientName_desc":
                    invoiceQuery = invoiceQuery.OrderByDescending(s => s.RecipientName);
                    break;
                case "origin":
                    invoiceQuery = invoiceQuery.OrderBy(s => s.Origin);
                    break;
                case "origin_desc":
                    invoiceQuery = invoiceQuery.OrderByDescending(s => s.Origin);
                    break;
                case "destination":
                    invoiceQuery = invoiceQuery.OrderBy(s => s.Destination);
                    break;
                case "destination_desc":
                    invoiceQuery = invoiceQuery.OrderByDescending(s => s.Destination);
                    break;
                case "toalInvoiceAmount":
                    invoiceQuery = invoiceQuery.OrderBy(s => s.TotalAmountPayable);
                    break;
                case "toalInvoiceAmount_desc":
                    invoiceQuery = invoiceQuery.OrderByDescending(s => s.TotalAmountPayable);
                    break;
                default:
                    invoiceQuery = invoiceQuery.OrderBy(s => s.WaybillNumber);
                    break;
            }

            invoiceSearch.Invoices = invoiceQuery.ToPagedList(pageNumber, pageSize);
            return View(invoiceSearch);
        }

        public bool SendInvoice(int? WaybillId)
        {
            var shipment = db.Shipments.FirstOrDefault(s => s.WaybillId == WaybillId);
            if (shipment == null)
            {
                return false;
            }

            // duty & tax not yet entered
            if (!shipment.TaxEntered == true)
                return false;

            // actual weight not yet entered
            if (!shipment.WeightEntered == true)
                return false;

            var shipmentInvoice = shipment.Invoices.FirstOrDefault(a => a.Type == "shipment");
            var dutyAndTaxInvoice = shipment.Invoices.FirstOrDefault(a => a.Type == "tax_duty");

            bool seperateInvoice = (shipmentInvoice.ShippingAccountNumber != dutyAndTaxInvoice.ShippingAccountNumber);

            // authorization codes not yet entered
            if (seperateInvoice)
            {
                if (string.IsNullOrWhiteSpace(shipmentInvoice.AuthenticationCode) ||
                    string.IsNullOrWhiteSpace(dutyAndTaxInvoice.AuthenticationCode))
                    return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(shipmentInvoice.AuthenticationCode))
                    return false;
            }

            // Shipment Invoice
            string shipmentPayerShippingAccountNumber = shipmentInvoice.ShippingAccountNumber;
            var shipmentPayerShippingAccount = db.ShippingAccounts.FirstOrDefault(s => s.ShippingAccountNumber == shipmentPayerShippingAccountNumber);
            string shipmentPayerCurrencyCode = db.Destinations.FirstOrDefault(s => s.ProvinceCode == shipmentPayerShippingAccount.ProvinceCode).CurrencyCode;

            // calculate package costs
            int i = 0;
            decimal totalShipmentCost = 0;
            List<ItemRow> shipmentItems = new List<ItemRow>();
            foreach (var package in shipment.Packages)
            {
                i++;
                string packageInfo = "Package Type: " + package.PackageTypeSize
                    + "\nActual Weight: " + package.WeightActual;
                decimal actualWeight = (decimal)package.WeightActual;

                Dictionary<string, decimal> cost = Calculate(package.Shipment.ServiceType, package.PackageTypeSize, actualWeight);
                totalShipmentCost += cost[shipmentPayerCurrencyCode];
                shipmentItems.Add(ItemRow.Make("Package " + i.ToString(), packageInfo, (decimal)1, 0, cost[shipmentPayerCurrencyCode], cost[shipmentPayerCurrencyCode]));
            }

            // save total cost to shipment invoice
            shipmentInvoice.TotalAmountPayable = (double) totalShipmentCost;
            db.Entry(shipmentInvoice).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            string[] CompanyAddress = { "Sino Express LLC", "HKUST" };

            // Shipment Payer Info
            string[] shipmentPayerInfo = new string[5];
            if (shipmentPayerShippingAccount is BusinessShippingAccount)
            {
                var shipmentPayerBusinessShippingAccount = (BusinessShippingAccount)shipmentPayerShippingAccount;
                shipmentPayerInfo[0] = shipmentPayerBusinessShippingAccount.CompanyName;
            }
            else
            {
                var shipmentPayerPersonalShippingAccount = (PersonalShippingAccount)shipmentPayerShippingAccount;
                shipmentPayerInfo[0] = shipmentPayerPersonalShippingAccount.FirstName + " " + shipmentPayerPersonalShippingAccount.LastName;
            }
            shipmentPayerInfo[1] = "Shipping Account Number: " + shipmentPayerShippingAccount.ShippingAccountNumber;
            shipmentPayerInfo[2] = "Credit Card Type: " + shipmentPayerShippingAccount.CardType;
            string shipmentPayerCreditCardNumber = shipmentPayerShippingAccount.CardNumber;
            shipmentPayerInfo[3] = "Credit Card Number (Last Four Digits): " + shipmentPayerCreditCardNumber.Substring(shipmentPayerCreditCardNumber.Length - 4);
            shipmentPayerInfo[4] = "Authorization Code: " + shipmentInvoice.AuthenticationCode;

            // Shipment Info
            string waybillNumber = db.Shipments.FirstOrDefault(a => a.WaybillId == WaybillId).WaybillNumber;
            string[] shipmentInfo = new string[8];
            shipmentInfo[0] = "Waybill Number: " + waybillNumber;
            shipmentInfo[1] = "Shipped Date: " + shipment.ShippedDate.ToString();
            shipmentInfo[2] = "Service Type: " + shipment.ServiceType;
            shipmentInfo[3] = "Sender's Reference Number: " + shipment.ReferenceNumber;
            var senderShippingAccount = shipment.ShippingAccount;
            string firstName = "";
            if (senderShippingAccount is BusinessShippingAccount)
            {
                var senderBusinessShippingAccount = (BusinessShippingAccount)senderShippingAccount;
                shipmentInfo[4] = "Sender Company: " + senderBusinessShippingAccount.CompanyName
                    + " (Contact Person: " + senderBusinessShippingAccount.ContactPersonName + ")";
                firstName = senderBusinessShippingAccount.CompanyName;
            }
            else
            {
                var senderPersonalShippingAccount = (PersonalShippingAccount)senderShippingAccount;
                shipmentInfo[4] = "Sender's Full Name: " + senderPersonalShippingAccount.FirstName + " " + senderPersonalShippingAccount.LastName;
                firstName = senderPersonalShippingAccount.FirstName;
            }
            shipmentInfo[5] = "Sender's Address: " + ((string.IsNullOrWhiteSpace(senderShippingAccount.BuildingInformation)) ? "" : senderShippingAccount.BuildingInformation + ", ")
                + senderShippingAccount.StreetInformation + ", "
                + senderShippingAccount.City + ", "
                + senderShippingAccount.ProvinceCode + ", "
                + senderShippingAccount.PostalCode;

            var recipient = shipment.Recipient;
            shipmentInfo[6] = "Recipient's Full Name: " + recipient.FullName;
            shipmentInfo[7] = "Recipient's Address: " + ((string.IsNullOrWhiteSpace(recipient.Building)) ? "" : recipient.Building + ", ")
                + recipient.Street + ", "
                + recipient.City + ", "
                + recipient.ProvinceCode + ", "
                + recipient.PostalCode;

            double dutyAmount = dutyAndTaxInvoice.Duty;
            double taxAmount = dutyAndTaxInvoice.Tax;
            string invoiceFolder = Server.MapPath("~/Invoices");

            if (seperateInvoice)
            {
                // -------------------------------
                // Shipment Invoice
                // -------------------------------
                try
                {
                    new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, shipmentPayerCurrencyCode + " ")
                    .TextColor("#CC0000")
                    .BackColor("#FFD6CC")
                    .Reference(waybillNumber)
                    //.Image(@"company.jpg", 125, 27)
                    .Company(Address.Make("FROM", CompanyAddress))
                    .Client(Address.Make("BILLING TO", shipmentPayerInfo))
                    .Items(shipmentItems)
                    .Totals(new List<TotalRow> {
                        //TotalRow.Make("Sub Total", (decimal)totalShipmentCost),
                        TotalRow.Make("Total", (decimal)totalShipmentCost, true),
                    })
                    .Details(new List<DetailRow> {
                        DetailRow.Make("SHIPMENT INFORMATION", shipmentInfo)
                    })
                    .Save(Path.Combine(invoiceFolder, waybillNumber + "_shipment.pdf"));
                }
                catch (Exception e)
                { Console.WriteLine("{0} Exception caught.", e); }

                // Send Email
                try
                {
                    MailMessage mailMessage = new MailMessage();
                    //Add recipients 
                    mailMessage.To.Add(shipmentPayerShippingAccount.EmailAddress);

                    //Setting the displayed email address and display name
                    //!!!Do not use this to prank others!!!
                    //mailMessage.From = new MailAddress("invoice@sinex.com", "SinEx Invoices");

                    //Subject and content of the email
                    mailMessage.Subject = "E-Invoice for Your Shipment (Waybill No. " + waybillNumber + ")";
                    mailMessage.Body = "Dear " + shipmentPayerInfo[0] + ",\n \nPlease find attached the invoice for your Sino Express shipment (Waybill Number: " + waybillNumber + "). Thanks for choosing SinEx! \n \nBest Regards, \nSino Express Invoicing System";
                    mailMessage.Priority = MailPriority.Normal;
                    mailMessage.Attachments.Add(new Attachment(Path.Combine(invoiceFolder, waybillNumber + "_shipment.pdf")));

                    //Instantiate a new SmtpClient instance
                    SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");

                
                    //WARNING: DO NOT set any credentials and other settings

                    //Send
                    smtpClient.Send(mailMessage);
                    //Response.Write("Email Sent!!! Yay!");
                }
                catch (Exception e)
                { Console.WriteLine("{0} Exception caught.", e); }

                // -------------------------------
                // Duty and Tax Invoice
                // -------------------------------
                string dutyAndTaxPayerShippingAccountNumber = dutyAndTaxInvoice.ShippingAccountNumber;
                var dutyAndTaxPayerShippingAccount = db.ShippingAccounts.FirstOrDefault(s => s.ShippingAccountNumber == dutyAndTaxPayerShippingAccountNumber);
                string dutyAndTaxPayerCurrencyCode = db.Destinations.FirstOrDefault(s => s.ProvinceCode == dutyAndTaxPayerShippingAccount.ProvinceCode).CurrencyCode;

                // Duty and Tax Payer Info
                string[] dutyAndTaxPayerInfo = new string[5];
                if (dutyAndTaxPayerShippingAccount is BusinessShippingAccount)
                {
                    var dutyAndTaxPayerBusinessShippingAccount = (BusinessShippingAccount)dutyAndTaxPayerShippingAccount;
                    dutyAndTaxPayerInfo[0] = dutyAndTaxPayerBusinessShippingAccount.CompanyName;
                }
                else
                {
                    var dutyAndTaxPayerPersonalShippingAccount = (PersonalShippingAccount)dutyAndTaxPayerShippingAccount;
                    dutyAndTaxPayerInfo[0] = dutyAndTaxPayerPersonalShippingAccount.FirstName + " " + dutyAndTaxPayerPersonalShippingAccount.LastName;
                }
                dutyAndTaxPayerInfo[1] = "Shipping Account Number: " + dutyAndTaxPayerShippingAccount.ShippingAccountNumber;
                dutyAndTaxPayerInfo[2] = "Credit Card Type: " + dutyAndTaxPayerShippingAccount.CardType;
                string dutyAndTaxPayerCreditCardNumber = dutyAndTaxPayerShippingAccount.CardNumber;
                dutyAndTaxPayerInfo[3] = "Credit Card Number (Last Four Digits): " + dutyAndTaxPayerCreditCardNumber.Substring(dutyAndTaxPayerCreditCardNumber.Length - 4);
                dutyAndTaxPayerInfo[4] = "Authorization Code: " + dutyAndTaxInvoice.AuthenticationCode;

                double exchangeRate = db.Currencies.FirstOrDefault(a => a.CurrencyCode == dutyAndTaxPayerCurrencyCode).ExchangeRate;

                // Duty and Tax Invoice
                try
                {
                    new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, dutyAndTaxPayerCurrencyCode + " ")
                    .TextColor("#CC0000")
                    .BackColor("#FFD6CC")
                    .Reference(waybillNumber)
                    //.Image(@"company.jpg", 125, 27)
                    .Company(Address.Make("FROM", CompanyAddress))
                    .Client(Address.Make("BILLING TO", dutyAndTaxPayerInfo))
                    .Items(new List<ItemRow> {
                        ItemRow.Make("Duty", "", (decimal)1, 0, (decimal)(dutyAmount * exchangeRate), (decimal)(dutyAmount * exchangeRate)),
                        ItemRow.Make("Tax", "", (decimal)1, 0, (decimal)taxAmount, (decimal)taxAmount),
                    })
                    .Totals(new List<TotalRow> {
                        //TotalRow.Make("Sub Total", (decimal)dutyAndTaxAmount),
                        TotalRow.Make("Total", (decimal)(dutyAmount + taxAmount), true),
                    })
                    .Details(new List<DetailRow> {
                        DetailRow.Make("SHIPMENT INFORMATION", shipmentInfo)
                    })
                    .Save(Path.Combine(invoiceFolder, waybillNumber + "_duty_and_tax.pdf"));
                }
                catch (Exception e)
                { Console.WriteLine("{0} Exception caught.", e); }

                // combine and save total
                string filepath1 = Path.Combine(invoiceFolder, waybillNumber + "_shipment.pdf");
                string filepath2 = Path.Combine(invoiceFolder, waybillNumber + "_duty_and_tax.pdf");
                if (System.IO.File.Exists(filepath1) && System.IO.File.Exists(filepath2))
                {
                    using (PdfDocument one = PdfReader.Open(filepath1, PdfDocumentOpenMode.Import))
                    using (PdfDocument two = PdfReader.Open(filepath2, PdfDocumentOpenMode.Import))
                    using (PdfDocument outPdf = new PdfDocument())
                    {
                        CopyPages(one, outPdf);
                        CopyPages(two, outPdf);
                        try
                        {
                            outPdf.Save(Path.Combine(invoiceFolder, waybillNumber + "_total.pdf"));
                        }
                        catch (Exception e)
                        { Console.WriteLine("{0} Exception caught.", e); }
                    }
                }

                // Send Email
                try
                {
                    MailMessage mailMessage = new MailMessage();
                    //Add recipients 
                    mailMessage.To.Add(dutyAndTaxPayerShippingAccount.EmailAddress);

                    //Setting the displayed email address and display name
                    //!!!Do not use this to prank others!!!
                    mailMessage.From = new MailAddress("invoice@sinex.com", "SinEx Invoices");

                    //Subject and content of the email
                    mailMessage.Subject = "E-Invoice for Your Shipment (Waybill No. " + waybillNumber + ")";
                    mailMessage.Body = "Dear " + dutyAndTaxPayerInfo[0] + ",\n \nPlease find attached the invoice for your Sino Express shipment (Waybill Number: " + waybillNumber + "). Thanks for choosing SinEx! \n \nBest Regards, \nSino Express Invoicing System";
                    mailMessage.Priority = MailPriority.Normal;
                    mailMessage.Attachments.Add(new Attachment(Path.Combine(invoiceFolder, waybillNumber + "_duty_and_tax.pdf")));

                    //Instantiate a new SmtpClient instance
                    SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");

                    //WARNING: DO NOT set any credentials and other settings!!!

                    //Send
                    smtpClient.Send(mailMessage);
                    //Response.Write("Email Sent!!! Yay!");
                }
                catch (Exception e)
                { Console.WriteLine("{0} Exception caught.", e); }
            }
            else
            {
                // -----------------------------------
                // Shipment and Duty and Tax Invoice
                // -----------------------------------
                double exchangeRate = db.Currencies.FirstOrDefault(a => a.CurrencyCode == shipmentPayerCurrencyCode).ExchangeRate;

                shipmentItems.Add(ItemRow.Make("Duty", "", (decimal)1, 0, (decimal)(dutyAmount * exchangeRate), (decimal)(dutyAmount * exchangeRate)));
                shipmentItems.Add(ItemRow.Make("Tax", "", (decimal)1, 0, (decimal)(taxAmount * exchangeRate), (decimal)(taxAmount * exchangeRate)));

                try
                {
                    new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, shipmentPayerCurrencyCode + " ")
                    .TextColor("#CC0000")
                    .BackColor("#FFD6CC")
                    .Reference(waybillNumber)
                    //.Image(@"company.jpg", 125, 27)
                    .Company(Address.Make("FROM", CompanyAddress))
                    .Client(Address.Make("BILLING TO", shipmentPayerInfo))
                    .Items(shipmentItems)
                    .Totals(new List<TotalRow> {
                        //TotalRow.Make("Sub Total", (decimal)dutyAndTaxAmount),
                        TotalRow.Make("Total", (decimal)(totalShipmentCost + (decimal)((dutyAmount + taxAmount) * exchangeRate)), true),
                    })
                    .Details(new List<DetailRow> {
                        DetailRow.Make("SHIPMENT INFORMATION", shipmentInfo)
                    })
                    .Save(Path.Combine(invoiceFolder, waybillNumber + "_total.pdf"));
                }
                catch (Exception e)
                { Console.WriteLine("{0} Exception caught.", e); }
                

                // Send Email
                try
                {
                    MailMessage mailMessage = new MailMessage();
                    //Add recipients
                    mailMessage.To.Add(shipmentPayerShippingAccount.EmailAddress);
                    //mailMessage.To.Add("gqi@ust.hk");
                    //mailMessage.To.Add("xduac@ust.hk");
                    //mailMessage.To.Add("zyuaf@ust.hk");
                    //mailMessage.To.Add("swuai@ust.hk");

                    //Setting the displayed email address and display name
                    //!!!Do not use this to prank others!!!
                    mailMessage.From = new MailAddress("comp3111_team108@cse.ust.hk", "SinEx Invoices");

                    //Subject and content of the email
                    mailMessage.Subject = "E-Invoice for Your Shipment (Waybill No. " + waybillNumber + ")";
                    mailMessage.Body = "Dear " + shipmentPayerInfo[0] + ",\n \nPlease find attached the invoice for your Sino Express shipment (Waybill Number: " + waybillNumber + "). Thanks for choosing SinEx! \n \nBest Regards, \nSino Express Invoicing System";
                    mailMessage.Priority = MailPriority.Normal;
                    mailMessage.Attachments.Add(new Attachment(Path.Combine(invoiceFolder, waybillNumber + "_total.pdf")));

                    //Instantiate a new SmtpClient instance
                    SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");

                    //WARNING: DO NOT set any credentials and other settings!!!

                    //Send
                    smtpClient.Send(mailMessage);
                    //Response.Write("Email Sent!!! Yay!");
                }
                catch (Exception e)
                { Console.WriteLine("{0} Exception caught.", e); return false; }
            }

            shipment.Status = "invoice_sent";
            db.Entry(shipment).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            { Console.WriteLine("{0} Exception caught.", e); return false; }

            return true;
        }

        public ActionResult DisplayInvoice(string WaybillNumber)
        {
            var shipment = db.Shipments.FirstOrDefault(a => a.WaybillNumber == WaybillNumber);
            string invoiceFolder = Server.MapPath("~/Invoices");
            string invoicePath = Path.Combine(invoiceFolder, WaybillNumber + "_total.pdf");
            if (System.IO.File.Exists(invoicePath))
            {
                return File(invoicePath, "application/pdf");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

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
            // int serviceTypeID = db.ServiceTypes.FirstOrDefault(s => s.Type == ServiceType).ServiceTypeID;
            // int packageTypeID = db.PackageTypes.FirstOrDefault(s => s.Type == PackageType).PackageTypeID;
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
            double HKDRate = db.Currencies.FirstOrDefault(s => s.CurrencyCode == "HKD").ExchangeRate;
            double MOPRate = db.Currencies.FirstOrDefault(s => s.CurrencyCode == "MOP").ExchangeRate;
            double TWDRate = db.Currencies.FirstOrDefault(s => s.CurrencyCode == "TWD").ExchangeRate;
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

        void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
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
            var shippingAccountQuery = db.Shipments.Where(a => a.ShippingAccount.UserName == User.Identity.Name).Select(a => a.ShippingAccount.ShippingAccountNumber).Distinct().OrderBy(c => c);
            return new SelectList(shippingAccountQuery);
        }
        // GET: Shipments
        [Authorize(Roles = "Employee, Customer")]
        public ActionResult Index(string EmployeeAction, string ShippingAccountNumber, string ShippedStartDate, string ShippedEndDate)
        {
            IQueryable<Shipment> shipments;
            if (User.IsInRole("Customer"))
            {
                shipments = from s in db.Shipments
                            where s.ShippingAccount.UserName == User.Identity.Name
                            select s;
                return View(shipments.ToList());
            }

            // else Employee
            if (string.IsNullOrWhiteSpace(ShippingAccountNumber))
                shipments = from s in db.Shipments select s;
            else
                shipments = from s in db.Shipments where s.ShippingAccount.ShippingAccountNumber == ShippingAccountNumber select s;

            if (EmployeeAction == "enter_weight")
            {
                shipments = from s in shipments
                            where s.Status == "picked_up" && s.WeightEntered == false
                            select s;
            }

            else if (EmployeeAction == "enter_tax")
            {
                shipments = from s in shipments
                            where s.Status == "picked_up" && s.TaxEntered == false
                            select s;
            }

            //if (shipments != null)
            //{
                return View(shipments.ToList());
            //}
            //return View();
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
            var shippingAccount = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First();

            ViewBag.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(a => a.size).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            ViewBag.SavedAddresses = db.SavedAddresses.Where(a => a.ShippingAccountId == shippingAccount.ShippingAccountId).Select(a => a.NickName).ToList();
            ViewBag.Origin = shippingAccount.City;
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
            var shippingAccount = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First();
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            ViewBag.Origin = (from s in db.ShippingAccounts
                                   where s.UserName == User.Identity.Name
                                   select s).First().City;
            ViewBag.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(a => a.size).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();
            ViewBag.SavedAddresses = db.SavedAddresses.Where(a => a.ShippingAccountId == shippingAccount.ShippingAccountId).Select(a => a.NickName).ToList();
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
            // if (ModelState.IsValid)
            // {
                if (shipmentView.ShipmentPayer == "Recipient" || shipmentView.TaxPayer == "Recipient")
                {
                    if (string.IsNullOrWhiteSpace(shipmentView.RecipientShippingAccountNumber))
                    {
                        ViewBag.errorMessage = "Recipient's Shipping Account Number is required once selected as payer";
                        return View(shipmentView);
                    }
                }
                string DestinationCity = shipmentView.Shipment.Destination;
                List<string> cities = db.Destinations.Select(a => a.City).ToList();
                if (!ValidCity(DestinationCity))
                {
                    ViewBag.errorMessage = "Please input a valid city";
                    return View(shipmentView);
                }
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
                /* saved recipient address */
                var address = (from s in db.SavedAddresses
                               where s.NickName == shipmentView.Nickname
                               select s).FirstOrDefault();
                if (shipmentView.IsSavedRecipient == "saved_address")
                {
                    shipment.Recipient.Building = address.Building;
                    shipment.Recipient.City = address.City;
                    shipment.Recipient.Street = address.Street;
                    shipment.Recipient.ProvinceCode = address.ProvinceCode;
                    shipment.Recipient.PostalCode = address.PostalCode;
                }

                /* bind shipping account */
                shipment.ShippingAccountId = shippingAccount.ShippingAccountId;

                /* create invoices */
                if (shipmentView.RecipientShippingAccountNumber != null)
                {
                    var recipientPayer = (from s in db.ShippingAccounts
                                          where s.ShippingAccountNumber == shipmentView.RecipientShippingAccountNumber
                                          select s).FirstOrDefault();
                    if (recipientPayer == null)
                    {
                        ViewBag.errorMessage = "recipient number not found!";
                        // ModelState.AddModelError("recipientPayer", "recipient not found");
                        return View(shipmentView);
                    }
                }
                shipment.Invoices = new List<SinExWebApp20256461.Models.Invoice>();
                var shipmentInvoice = new SinExWebApp20256461.Models.Invoice { Type = "shipment" };
                shipmentInvoice.ShippingAccountNumber = ShipmentPayer == "Sender" ? shippingAccount.ShippingAccountNumber : shipmentView.RecipientShippingAccountNumber;
                var taxInvoice = new SinExWebApp20256461.Models.Invoice { Type = "tax_duty" };
                taxInvoice.ShippingAccountNumber = TaxPayer == "Sender" ? shippingAccount.ShippingAccountNumber : shipmentView.RecipientShippingAccountNumber;
                shipment.Invoices.Add(shipmentInvoice);
                shipment.Invoices.Add(taxInvoice);
                          

                shipment.IfSendEmail = shipmentView.IfSendEmail == "Yes" ? true : false;
                shipment.IfSendEmailRecipient = shipmentView.IfSendEmailRecipient == "Yes" ? true : false;

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
                //return RedirectToAction("Create", "Pickups", new { waybillId = shipment.WaybillId });
            // }

            // return View(shipmentView);
        }

        // GET: Shipments/Edit
        public ActionResult DisplayShipment(string WaybillNumber)
        {
            return EditCustomer(db.Shipments.FirstOrDefault(a => a.WaybillNumber == WaybillNumber).WaybillId);
        }

        // GET: Shipments/Edit
        public ActionResult EditCustomer(int? id)
        {
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            Shipment shipment = db.Shipments.Find(id);
            if (shipment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string shipmentPayer = "Sender", taxPayer = "Sender"; // set default value to senders
            int i_id1 = shipment.Invoices.FirstOrDefault(a => a.Type == "shipment").InvoiceID;
            int i_id2 = shipment.Invoices.FirstOrDefault(a => a.Type == "tax_duty").InvoiceID;
            var shipmentInvoice = db.Invoices.Find(i_id1);
            var taxInvoice = db.Invoices.Find(i_id2);
            string shipmentShippingAccountNumber = db.ShippingAccounts.FirstOrDefault(a => a.ShippingAccountId == shipment.ShippingAccountId).ShippingAccountNumber;

            string payerRecipient = "";
            if (!string.IsNullOrWhiteSpace(shipmentInvoice.ShippingAccountNumber))
            {
                if (shipmentInvoice.ShippingAccountNumber != shipmentShippingAccountNumber)
                {
                    shipmentPayer = "Recipient";
                    payerRecipient = shipmentInvoice.ShippingAccountNumber;
                }   
            }
            if (!string.IsNullOrWhiteSpace(taxInvoice.ShippingAccountNumber))
            {
                if (taxInvoice.ShippingAccountNumber != shipmentShippingAccountNumber)
                {
                    taxPayer = "Recipient";
                    payerRecipient = taxInvoice.ShippingAccountNumber;
                }
            }

            /* display the estimated fee */
            string currencyCode = db.Destinations.FirstOrDefault(a => a.ProvinceCode == shipment.ShippingAccount.ProvinceCode).CurrencyCode;
            var feeArray = new decimal[shipment.NumberOfPackages];
            decimal totalEstimatedCost = 0;
            for (int num = 0; num < shipment.NumberOfPackages; num++)
            {
                var fee = Calculate(shipment.ServiceType, shipment.Packages.ToList()[num].PackageTypeSize, (decimal)shipment.Packages.ToList()[num].WeightEstimated);
                feeArray[num] = fee[currencyCode];
                totalEstimatedCost += feeArray[num];
            }

            double exchangeRate = db.Currencies.FirstOrDefault(a => a.CurrencyCode == currencyCode).ExchangeRate;
            double dutyAmount = taxInvoice.Duty * exchangeRate;
            double taxAmount = taxInvoice.Tax * exchangeRate;

            var ret = new CreateShipmentViewModel
            {
                Shipment = shipment,
                DutyAmount = dutyAmount,
                TaxAmount = taxAmount,
                Packages = shipment.Packages.ToList(),
                IfSendEmail = (shipment.IfSendEmail) ? "Yes" : "No",
                IfSendEmailRecipient = (shipment.IfSendEmailRecipient) ? "Yes" : "No",
                ShipmentPayer = shipmentPayer,
                TaxPayer = taxPayer,
                ShipmentAuthorizationCode = shipment.Invoices.FirstOrDefault(a => a.Type == "shipment").AuthenticationCode,
                DutyAndTaxAuthorizationCode = shipment.Invoices.FirstOrDefault(a => a.Type == "tax_duty").AuthenticationCode,
                PackageTypeSizesList = new SelectList(db.PakageTypeSizes.Select(a => a.size).Distinct()),
                CurrenciesList = new SelectList(db.Currencies.Select(a => a.CurrencyCode).Distinct()),
                TaxCurrency = currencyCode,
            };

            if (payerRecipient != "")
            {
                ret.RecipientShippingAccountNumber = payerRecipient;
            }

            ViewBag.TaxCurrencyCodes = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();
            ViewBag.CurrencyCode = currencyCode;
            ViewBag.FeeCollection = feeArray;
            ViewBag.TotalEstimatedFee = totalEstimatedCost;
            ViewBag.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            return View(ret);
        }

        // POST: Shipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustomer(int? id, CreateShipmentViewModel shipmentView, string submit, string IfSendEmail, string IfSendEmailRecipient, string ShipmentPayer, string TaxPayer)
        {
            ViewBag.Cities = db.Destinations.Select(a => a.City).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(m => m.CurrencyCode).Distinct().ToList();
            ViewBag.ServiceTypes = db.ServiceTypes.Select(m => m.Type).Distinct().ToList();
            ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(m => m.size).Distinct().ToList();

            shipmentView.PackageTypeSizesList = new SelectList(db.PakageTypeSizes.Select(a => a.size).Distinct());
            shipmentView.CurrenciesList = new SelectList(db.Currencies.Select(a => a.CurrencyCode).Distinct());

            if (ModelState.IsValid)
            {
                var shipment = shipmentView.Shipment;
                var shipmentDB = db.Shipments.Find(id);
                if (!ValidCity(shipmentView.Shipment.Destination))
                {
                    ViewBag.errorMessage = "Please input a valid city";
                    return View(shipmentView);
                }
                if (submit == "add" && shipmentView.Packages.Count < 10)
                {
                    var new_package = new Package();
                    new_package.PackageTypeSize = "";
                    shipmentView.Packages.Add(new_package);
                    return View(shipmentView);
                }
                else if (submit.Contains("delete"))
                {
                    if (shipmentView.Packages.Count > 1)
                    {
                        int _id = Int32.Parse(submit.Split(' ')[1]) - 1;
                        shipmentView.Packages.Remove(shipmentView.Packages[_id]);
                    }
                    return View(shipmentView);
                }

                if (IfSendEmail == "Yes")
                    shipment.IfSendEmail = true;
                else
                    shipment.IfSendEmail = false;

                if (IfSendEmailRecipient == "Yes")
                    shipment.IfSendEmailRecipient = true;
                else
                    shipment.IfSendEmailRecipient = false;

                /* Update Invoice */
                var _invoice1 = shipmentDB.Invoices.SingleOrDefault(c => c.Type == "shipment");
                var _invoice2 = shipmentDB.Invoices.SingleOrDefault(c => c.Type == "tax_duty");
                var a = shipmentDB.ShippingAccount.ShippingAccountNumber;
                if (ShipmentPayer == "Recipient" && string.IsNullOrWhiteSpace(shipmentView.RecipientShippingAccountNumber))
                    _invoice1.ShippingAccountNumber = shipmentView.RecipientShippingAccountNumber;
                if (TaxPayer == "Recipient" && string.IsNullOrWhiteSpace(shipmentView.RecipientShippingAccountNumber))
                    _invoice2.ShippingAccountNumber = shipmentView.RecipientShippingAccountNumber;

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
                _recipient.CompanyName = shipment.Recipient.CompanyName;
                _recipient.DeptName = shipment.Recipient.DeptName;

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
                shipmentDB.IfSendEmailRecipient = IfSendEmailRecipient == "Yes" ? true : false;


                // shipmentDB.Status = submit == "Confirm" ? "confirmed" : "pending";

                db.Entry(_invoice1).State = EntityState.Modified;
                db.Entry(_invoice2).State = EntityState.Modified;
                db.Entry(_recipient).State = EntityState.Modified;
                db.Entry(shipmentDB).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return View(shipmentView);
                }

                if (submit == "Continue")
                {
                   return RedirectToAction("Create", "Pickups", new { waybillId = shipmentDB.WaybillId });
                }
                return RedirectToAction("Index");
            }
            return View(shipmentView);
        }

        // GET: Shipments/Edit
        public ActionResult EnterWeight(int? id)
        {
            return EditCustomer(id);
        }

        // POST: Shipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnterWeight(int? id, CreateShipmentViewModel shipmentView, string submit, string IfSendEmail, string IfSendEmailRecipient, string ShipmentPayer, string TaxPayer)
        {
            if (ModelState.IsValid)
            {
                ViewBag.PackageCurrency = db.Currencies.Select(m => m.CurrencyCode).Distinct().ToList();
                ViewBag.ServiceTypes = db.ServiceTypes.Select(m => m.Type).Distinct().ToList();
                ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(m => m.size).Distinct().ToList();

                var shipmentDB = db.Shipments.Find(id);

                /* Packages */
                var old_packages = from s in db.Packages
                                    where s.WaybillId == shipmentDB.WaybillId
                                    select s;
                foreach (var i in old_packages)
                    db.Packages.Remove(i);

                for (int i = 0; i < shipmentView.Packages.Count; i++)
                {
                    shipmentDB.Packages.Add(shipmentView.Packages[i]);
                    try
                    {
                        db.Packages.Add(shipmentView.Packages[i]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return View(shipmentView);
                    }
                }

                shipmentDB.WeightEntered = true;

                db.Entry(shipmentDB).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return View(shipmentView);
                }

                bool invoice_sent = SendInvoice(id);
                return RedirectToAction("Index");
            }

            return View(shipmentView);
        }

        // GET: Shipments/Edit
        public ActionResult EnterTax(int? id)
        {
            return EditCustomer(id);
        }

        // POST: Shipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnterTax(int? id, CreateShipmentViewModel shipmentView, string submit, string IfSendEmail, string IfSendEmailRecipient, string ShipmentPayer, string TaxPayer)
        {
            if (ModelState.IsValid)
            {
                ViewBag.PackageCurrency = db.Currencies.Select(m => m.CurrencyCode).Distinct().ToList();
                ViewBag.ServiceTypes = db.ServiceTypes.Select(m => m.Type).Distinct().ToList();
                ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(m => m.size).Distinct().ToList();

                var shipmentDB = db.Shipments.Find(id);
                var shippingAccount = (from s in db.ShippingAccounts
                                       where s.UserName == User.Identity.Name
                                       select s).First();
                var shipmentInvoices = shipmentDB.Invoices;
                string payerRecipient = "";
                if (!string.IsNullOrWhiteSpace(shipmentInvoices.ToList()[0].ShippingAccountNumber))
                {
                    if (shipmentInvoices.ToList()[0].ShippingAccountNumber != shippingAccount.ShippingAccountNumber)
                    {
                        payerRecipient = shipmentInvoices.ToList()[0].ShippingAccountNumber;
                    }
                }
                if (!string.IsNullOrWhiteSpace(shipmentInvoices.ToList()[1].ShippingAccountNumber))
                {
                    if (shipmentInvoices.ToList()[1].ShippingAccountNumber != shippingAccount.ShippingAccountNumber)
                    {
                        payerRecipient = shipmentInvoices.ToList()[0].ShippingAccountNumber;
                    }
                }
                if (payerRecipient != "")
                {
                    shipmentView.RecipientShippingAccountNumber = payerRecipient;
                }

                /* Invoice */
                int i_id2 = shipmentDB.Invoices.FirstOrDefault(a => a.Type == "tax_duty").InvoiceID;
                var dutyAndTaxInvoice = db.Invoices.Find(i_id2);

                // shipment costs to be determined when sending shipment invoice
                var exchangeRate = db.Currencies.FirstOrDefault(a => a.CurrencyCode == shipmentView.TaxCurrency).ExchangeRate;

                dutyAndTaxInvoice.Duty = shipmentView.DutyAmount / exchangeRate;
                dutyAndTaxInvoice.Tax = shipmentView.TaxAmount / exchangeRate;
                dutyAndTaxInvoice.TotalAmountPayable = dutyAndTaxInvoice.Duty + dutyAndTaxInvoice.Tax;

                shipmentDB.TaxEntered = true;

                db.Entry(dutyAndTaxInvoice).State = EntityState.Modified;
                db.Entry(shipmentDB).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return View(shipmentView);
                }

                bool invoice_sent = SendInvoice(id);
                return RedirectToAction("Index");
            }

            return View(shipmentView);
        }

        // GET: Shipments/Edit
        public ActionResult EnterAuthorization(int? id)
        {
            return EditCustomer(id);
        }

        // POST: Shipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnterAuthorization(int? id, CreateShipmentViewModel shipmentView, string submit, string IfSendEmail, string IfSendEmailRecipient, string ShipmentPayer, string TaxPayer)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(shipmentView.ShipmentAuthorizationCode) &&
                    string.IsNullOrWhiteSpace(shipmentView.DutyAndTaxAuthorizationCode))
                    return RedirectToAction("Index");

                ViewBag.PackageCurrency = db.Currencies.Select(m => m.CurrencyCode).Distinct().ToList();
                ViewBag.ServiceTypes = db.ServiceTypes.Select(m => m.Type).Distinct().ToList();
                ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(m => m.size).Distinct().ToList();

                var shipmentDB = db.Shipments.Find(id);

                /* Invoice */
                int i_id1 = shipmentDB.Invoices.FirstOrDefault(a => a.Type == "shipment").InvoiceID;
                int i_id2 = shipmentDB.Invoices.FirstOrDefault(a => a.Type == "tax_duty").InvoiceID;
                var shipmentInvoice = db.Invoices.Find(i_id1);
                var dutyAndTaxInvoice = db.Invoices.Find(i_id2);

                shipmentInvoice.AuthenticationCode = shipmentView.ShipmentAuthorizationCode;
                dutyAndTaxInvoice.AuthenticationCode = shipmentView.DutyAndTaxAuthorizationCode;

                db.Entry(shipmentInvoice).State = EntityState.Modified;
                db.Entry(dutyAndTaxInvoice).State = EntityState.Modified;
                db.Entry(shipmentDB).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return View(shipmentView);
                }

                bool invoice_sent = SendInvoice(id);
                return RedirectToAction("Index");
            }

            return View(shipmentView);
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
