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

        public void SendInvoice(int? WaybillId)
        {
            var shipment = db.Shipments.SingleOrDefault(s => s.WaybillId == WaybillId);
            if (shipment == null)
            {
                return;
            }

            SinExWebApp20256461.Models.Invoice shipmentInvoice = null;
            SinExWebApp20256461.Models.Invoice dutyAndTaxInvoice = null;
            foreach (var invoice in shipment.Invoices)
            {
                if (invoice.Type == "shipment")
                    shipmentInvoice = invoice;
                else if (invoice.Type == "tax_duty")
                    dutyAndTaxInvoice = invoice;
            }

            string[] CompanyAddress = { "Sino Express LLC", "HKUST" };

            // Shipment Invoice
            string shipmentPayerShippingAccountNumber = shipmentInvoice.ShippingAccountNumber;
            var shipmentPayerShippingAccount = db.ShippingAccounts.SingleOrDefault(s => s.ShippingAccountNumber == shipmentPayerShippingAccountNumber);
            string shipmentPayerCurrencyCode = db.Destinations.SingleOrDefault(s => s.ProvinceCode == shipmentPayerShippingAccount.ProvinceCode).CurrencyCode;

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
            string waybillNumber = WaybillId.ToString().PadLeft(16, '0');
            string[] shipmentInfo = new string[6];
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
            shipmentInfo[5] = "Sender's Address: " + senderShippingAccount.BuildingInformation + ", "
                + senderShippingAccount.StreetInformation + ", "
                + senderShippingAccount.City + ", "
                + senderShippingAccount.ProvinceCode + ", "
                + senderShippingAccount.PostalCode;

            double dutyAmount = dutyAndTaxInvoice.Duty;
            double taxAmount = dutyAndTaxInvoice.Tax;
            string invoiceFolder = Server.MapPath("~/Invoices");

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


            bool seperateInvoice = (shipmentInvoice.ShippingAccountNumber != dutyAndTaxInvoice.ShippingAccountNumber);

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
                { }

                // Send Email
                try
                {
                    MailMessage mailMessage = new MailMessage();
                    //Add recipients 
                    mailMessage.To.Add(senderShippingAccount.EmailAddress);

                    //Setting the displayed email address and display name
                    //!!!Do not use this to prank others!!!
                    mailMessage.From = new MailAddress("invoice@sinex.com", "SinEx Invoices");

                    //Subject and content of the email
                    mailMessage.Subject = "E-Invoice for Your Shipment (Waybill No. " + waybillNumber + ")";
                    mailMessage.Body = "Dear " + firstName + ",\n \n Please find attached the invoice for your Sino Express shipment (Waybill Number: " + waybillNumber + "). Thanks for choosing SinEx! \n \n Best Regards, \n Sino Express Invoicing System";
                    mailMessage.Priority = MailPriority.Normal;
                    mailMessage.Attachments.Add(new Attachment(Path.Combine(invoiceFolder, waybillNumber + "_shipment.pdf")));

                    //Instantiate a new SmtpClient instance
                    SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");
                    smtpClient.Credentials = new System.Net.NetworkCredential("comp3111_team108@cse.ust.hk", "team108#");
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;
                    //Send
                    smtpClient.Send(mailMessage);
                    //Response.Write("Email Sent!!! Yay!");
                }
                catch (Exception e) { }

                // -------------------------------
                // Duty and Tax Invoice
                // -------------------------------
                string dutyAndTaxPayerShippingAccountNumber = dutyAndTaxInvoice.ShippingAccountNumber;
                var dutyAndTaxPayerShippingAccount = db.ShippingAccounts.SingleOrDefault(s => s.ShippingAccountNumber == dutyAndTaxPayerShippingAccountNumber);
                string dutyAndTaxPayerCurrencyCode = db.Destinations.SingleOrDefault(s => s.ProvinceCode == dutyAndTaxPayerShippingAccount.ProvinceCode).CurrencyCode;

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
                        ItemRow.Make("Duty", "", (decimal)1, 0, (decimal)dutyAmount, (decimal)dutyAmount),
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
                { }

                // Send Email
                try
                {
                    MailMessage mailMessage = new MailMessage();
                    //Add recipients 
                    mailMessage.To.Add(senderShippingAccount.EmailAddress);

                    //Setting the displayed email address and display name
                    //!!!Do not use this to prank others!!!
                    mailMessage.From = new MailAddress("invoice@sinex.com", "SinEx Invoices");

                    //Subject and content of the email
                    mailMessage.Subject = "E-Invoice for Your Shipment (Waybill No. " + waybillNumber + ")";
                    mailMessage.Body = "Dear " + firstName + ",\n \n Please find attached the invoice for your Sino Express shipment (Waybill Number: " + waybillNumber + "). Thanks for choosing SinEx! \n \n Best Regards, \n Sino Express Invoicing System";
                    mailMessage.Priority = MailPriority.Normal;
                    mailMessage.Attachments.Add(new Attachment(Path.Combine(invoiceFolder, waybillNumber + "_duty_and_tax.pdf")));

                    //Instantiate a new SmtpClient instance
                    SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");
                    smtpClient.Credentials = new System.Net.NetworkCredential("comp3111_team108@cse.ust.hk", "team108#");
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;
                    //Send
                    smtpClient.Send(mailMessage);
                    //Response.Write("Email Sent!!! Yay!");
                }
                catch (Exception e) { }


                string filepath1 = invoiceFolder + waybillNumber + "_shipment.pdf";
                string filepath2 = invoiceFolder + waybillNumber + "_duty_and_tax.pdf";
                if (System.IO.File.Exists(filepath1) && System.IO.File.Exists(filepath2))
                {
                    using (PdfDocument one = PdfReader.Open("file1.pdf", PdfDocumentOpenMode.Import))
                    using (PdfDocument two = PdfReader.Open("file2.pdf", PdfDocumentOpenMode.Import))
                    using (PdfDocument outPdf = new PdfDocument())
                    {
                        CopyPages(one, outPdf);
                        CopyPages(two, outPdf);
                        outPdf.Save(Path.Combine(invoiceFolder, waybillNumber + "_total.pdf"));
                    }
                }
            }
            else
            {
                // -----------------------------------
                // Shipment and Duty and Tax Invoice
                // -----------------------------------
                shipmentItems.Add(ItemRow.Make("Duty", "", (decimal)1, 0, (decimal)dutyAmount, (decimal)dutyAmount));
                shipmentItems.Add(ItemRow.Make("Tax", "", (decimal)1, 0, (decimal)taxAmount, (decimal)taxAmount));

                try
                {
                    new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, shipmentPayerCurrencyCode)
                    .TextColor("#CC0000")
                    .BackColor("#FFD6CC")
                    .Reference(waybillNumber)
                    //.Image(@"company.jpg", 125, 27)
                    .Company(Address.Make("FROM", CompanyAddress))
                    .Client(Address.Make("BILLING TO", shipmentPayerInfo))
                    .Items(shipmentItems)
                    .Totals(new List<TotalRow> {
                        //TotalRow.Make("Sub Total", (decimal)dutyAndTaxAmount),
                        TotalRow.Make("Total", (decimal)(totalShipmentCost + (decimal)(dutyAmount + taxAmount)), true),
                    })
                    .Details(new List<DetailRow> {
                        DetailRow.Make("SHIPMENT INFORMATION", shipmentInfo)
                    })
                    .Save(Path.Combine(invoiceFolder, waybillNumber + "_total.pdf"));
                }
                catch (Exception e)
                { }
                

                // Send Email
                try
                {
                    MailMessage mailMessage = new MailMessage();
                    //Add recipients 
                    //mailMessage.To.Add(senderShippingAccount.EmailAddress);
                    mailMessage.To.Add("gqi@connect.ust.hk");
                    mailMessage.To.Add("xduac@connect.ust.hk");
                    mailMessage.To.Add("zyuaf@connect.ust.hk");
                    mailMessage.To.Add("swuai@connect.ust.hk");

                    //Setting the displayed email address and display name
                    //!!!Do not use this to prank others!!!
                    mailMessage.From = new MailAddress("invoice@sinex.com", "SinEx Invoices");

                    //Subject and content of the email
                    mailMessage.Subject = "E-Invoice for Your Shipment (Waybill No. " + waybillNumber + ")";
                    mailMessage.Body = "Dear " + firstName + ",\n \n Please find attached the invoice for your Sino Express shipment (Waybill Number: " + waybillNumber + "). Thanks for choosing SinEx! \n \n Best Regards, \n Sino Express Invoicing System";
                    mailMessage.Priority = MailPriority.Normal;
                    mailMessage.Attachments.Add(new Attachment(Path.Combine(invoiceFolder, waybillNumber + "_total.pdf")));

                    //Instantiate a new SmtpClient instance
                    SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");
                    smtpClient.Credentials = new System.Net.NetworkCredential("comp3111_team108@cse.ust.hk", "team108#");
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;
                    //Send
                    smtpClient.Send(mailMessage);
                    //Response.Write("Email Sent!!! Yay!");
                }
                catch (Exception e) { Console.WriteLine("{0} Exception caught.", e); }

            }
        }

        public ActionResult DisplayInvoice(string WaybillNumber)
        {
            var shipment = db.Shipments.SingleOrDefault(a => a.WaybillNumber == WaybillNumber);
            string invoiceFolder = Server.MapPath("~/Invoices");
            string invoicePath = Path.Combine(invoiceFolder, WaybillNumber + "_total.pdf");
            if (System.IO.File.Exists(invoicePath))
            {
                return File(invoicePath, "application/pdf");
            }
            else
            {
                return new EmptyResult();
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
        public ActionResult Index(string EmployeeAction)
        {
            var shipments = from s in db.Shipments select s;
            if (User.IsInRole("Customer"))
            {
                var shippingAccount = db.ShippingAccounts.FirstOrDefault(s => s.UserName == User.Identity.Name);
                var shippingAccountId = 0;
                if (shippingAccount != null)
                {
                    shippingAccountId = shippingAccount.ShippingAccountId;
                }
                shipments = from s in shipments
                            where s.ShippingAccountId == shippingAccountId
                            select s;
                return View(shipments.ToList());
            }

            if (EmployeeAction == "enter_weight")
            {
                shipments = from s in shipments
                            where s.Status == "confirmed" || s.Status == "picked_up"
                            where s.Packages.Any(a => a.WeightActual.Equals(0))
                            select s;
            }

            else if (EmployeeAction == "enter_tax")
            {
                shipments = from s in shipments
                            where s.Status == "confirmed" || s.Status == "picked_up"
                            where s.Invoices.FirstOrDefault(a => a.Type == "tax_duty").Duty.Equals(0) ||
                                  s.Invoices.FirstOrDefault(a => a.Type == "tax_duty").Tax.Equals(0)
                            select s;
            }

            if (shipments != null)
            {
                View(shipments.ToList());
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
                shipment.Invoices = new List<SinExWebApp20256461.Models.Invoice>();
                var shipmentInvoice = new SinExWebApp20256461.Models.Invoice { Type = "shipment" };
                shipmentInvoice.ShippingAccountNumber = ShipmentPayer == "Sender" ? shippingAccount.ShippingAccountNumber : shipmentView.RecipientShippingAccountNumber;
                var taxInvoice = new SinExWebApp20256461.Models.Invoice { Type = "tax_duty" };
                taxInvoice.ShippingAccountNumber = TaxPayer == "Sender" ? shippingAccount.ShippingAccountNumber : shipmentView.RecipientShippingAccountNumber;
                shipment.Invoices.Add(shipmentInvoice);
                shipment.Invoices.Add(taxInvoice);
                          

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
                //return RedirectToAction("Create", "Pickups", new { waybillId = shipment.WaybillId });
            }

            return View(shipmentView);
        }

        // GET: Shipments/Edit
        public ActionResult EditCustomer(int? id)
        {
            Shipment shipment = db.Shipments.Find(id);
            if (shipment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string shipmentPayer;
            string taxPayer;
            int i_id1 = shipment.Invoices.SingleOrDefault(a => a.Type == "shipment").InvoiceID;
            int i_id2 = shipment.Invoices.SingleOrDefault(a => a.Type == "tax_duty").InvoiceID;
            var shipmentInvoice = db.Invoices.Find(i_id1);
            var taxInvoice = db.Invoices.Find(i_id2);
            string shipmentShippingAccountNumber = db.ShippingAccounts.SingleOrDefault(a => a.ShippingAccountId == shipment.ShippingAccountId).ShippingAccountNumber;
            if (string.IsNullOrWhiteSpace(shipmentInvoice.ShippingAccountNumber))
                shipmentPayer = "";
            else if (shipmentInvoice.ShippingAccountNumber == shipmentShippingAccountNumber)
                shipmentPayer = "Sender";
            else
                shipmentPayer = "Recipient";
            if (string.IsNullOrWhiteSpace(taxInvoice.ShippingAccountNumber))
                taxPayer = "";
            else if (taxInvoice.ShippingAccountNumber == shipmentShippingAccountNumber)
                taxPayer = "Sender";
            else
                taxPayer = "Recipient";
            var ret = new CreateShipmentViewModel {
                Shipment = shipment,
                DutyAmount = taxInvoice.Duty,
                TaxAmount = taxInvoice.Tax,
                Packages = shipment.Packages.ToList(),
                IfSendEmail = (shipment.IfSendEmail) ? "Yes" : "No",
                ShipmentPayer = shipmentPayer,
                TaxPayer = taxPayer,
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
        public ActionResult EditCustomer(int? id, CreateShipmentViewModel shipmentView, string submit, string IfSendEmail, string ShipmentPayer, string TaxPayer)
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


                shipmentDB.Status = submit == "Confirm" ? "confirmed" : "pending";

                db.Entry(_invoice1).State = EntityState.Modified;
                db.Entry(_invoice2).State = EntityState.Modified;
                db.Entry(_recipient).State = EntityState.Modified;
                db.Entry(shipmentDB).State = EntityState.Modified;
                db.SaveChanges();

                if (submit == "Confirm")
                {
                   return RedirectToAction("Create", "Pickups", new { waybillId = shipmentDB.WaybillId });
                }
            }        
            return RedirectToAction("Index");
        }

        // GET: Shipments/Edit
        public ActionResult EditEmployee(int? id)
        {
            Shipment shipment = db.Shipments.Find(id);
            if (shipment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string shipmentPayer;
            string taxPayer;
            int i_id1 = shipment.Invoices.SingleOrDefault(a => a.Type == "shipment").InvoiceID;
            int i_id2 = shipment.Invoices.SingleOrDefault(a => a.Type == "tax_duty").InvoiceID;
            var shipmentInvoice = db.Invoices.Find(i_id1);
            var taxInvoice = db.Invoices.Find(i_id2);
            string shipmentShippingAccountNumber = db.ShippingAccounts.SingleOrDefault(a => a.ShippingAccountId == shipment.ShippingAccountId).ShippingAccountNumber;
            if (string.IsNullOrWhiteSpace(shipmentInvoice.ShippingAccountNumber))
                shipmentPayer = "";
            else if (shipmentInvoice.ShippingAccountNumber == shipmentShippingAccountNumber)
                shipmentPayer = "Sender";
            else
                shipmentPayer = "Recipient";
            if (string.IsNullOrWhiteSpace(taxInvoice.ShippingAccountNumber))
                taxPayer = "";
            else if (taxInvoice.ShippingAccountNumber == shipmentShippingAccountNumber)
                taxPayer = "Sender";
            else
                taxPayer = "Recipient";

            var ret = new CreateShipmentViewModel
            {
                Shipment = shipment,
                Status = shipment.Status,
                DutyAmount = taxInvoice.Duty,
                TaxAmount = taxInvoice.Tax,
                Packages = shipment.Packages.ToList(),
                IfSendEmail = (shipment.IfSendEmail) ? "Yes" : "No",
                ShipmentPayer = shipmentPayer,
                TaxPayer = taxPayer,
            };
            ViewBag.ServiceTypes = db.ServiceTypes.Select(a => a.Type).Distinct().ToList();
            ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(a => a.size).Distinct().ToList();
            ViewBag.PackageCurrency = db.Currencies.Select(a => a.CurrencyCode).Distinct().ToList();

            switch (shipment.Status)
            {
                case "confirmed":
                    ViewBag.Statuses = new List<string>{ "picked_up" };
                    break;
                case "picked_up":
                    ViewBag.Statuses = new List<string>{ "picked_up", "lost" };
                    break;
                case "pending":
                case "invoice_sent":
                case "delivered":
                case "lost":
                case "cancelled":
                default:
                    break;
            }

            return View(ret);
        }

        // POST: Shipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEmployee(int? id, CreateShipmentViewModel shipmentView, string submit, string IfSendEmail, string ShipmentPayer, string TaxPayer)
        {
            if (ModelState.IsValid)
            {
                bool sendInvoice = true;

                ViewBag.PackageCurrency = db.Currencies.Select(m => m.CurrencyCode).Distinct().ToList();
                ViewBag.ServiceTypes = db.ServiceTypes.Select(m => m.Type).Distinct().ToList();
                ViewBag.PackageTypeSizes = db.PakageTypeSizes.Select(m => m.size).Distinct().ToList();

                var shipment = shipmentView.Shipment;
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
                    db.Packages.Add(shipmentView.Packages[i]);
                    if (shipmentView.Packages[i].WeightActual.Equals(0))
                        sendInvoice = false;
                }
                shipmentDB.NumberOfPackages = shipmentView.Packages.Count;
                shipmentDB.Status = shipmentView.Status;

                /* Invoice */
                int i_id1 = shipmentDB.Invoices.SingleOrDefault(a => a.Type == "shipment").InvoiceID;
                int i_id2 = shipmentDB.Invoices.SingleOrDefault(a => a.Type == "tax_duty").InvoiceID;
                var shipmentInvoice = db.Invoices.Find(i_id1);
                var dutyAndTaxInvoice = db.Invoices.Find(i_id2);


                if (shipmentInvoice.ShippingAccountNumber == dutyAndTaxInvoice.ShippingAccountNumber)
                {
                    if (string.IsNullOrWhiteSpace(shipmentView.ShipmentAuthorizationCode))
                    {
                        sendInvoice = false;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(shipmentView.ShipmentAuthorizationCode) ||
                        string.IsNullOrWhiteSpace(shipmentView.DutyAndTaxAuthorizationCode))
                    {
                        sendInvoice = false;
                    }
                }

                // If not yet picked up, authorization codes will not be generated
                if (shipmentView.Status == "picked_up")
                {
                    shipmentInvoice.AuthenticationCode = shipmentView.ShipmentAuthorizationCode;
                    dutyAndTaxInvoice.AuthenticationCode = shipmentView.DutyAndTaxAuthorizationCode;
                }

                // shipment costs to be determined when sending shipment invoice
                dutyAndTaxInvoice.Duty = shipmentView.DutyAmount;
                dutyAndTaxInvoice.Tax = shipmentView.TaxAmount;

                if (shipmentView.DutyAmount.Equals(0) || shipmentView.TaxAmount.Equals(0))
                    sendInvoice = false;

                if (sendInvoice)
                    SendInvoice(id);

                shipment.Status = "invoice_sent";

                db.Entry(shipmentInvoice).State = EntityState.Modified;
                db.Entry(dutyAndTaxInvoice).State = EntityState.Modified;
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
