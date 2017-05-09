using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SinExWebApp20256461.Models;
using SinExWebApp20256461.ViewModels;

namespace SinExWebApp20256461.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        private SinExWebApp20256461Context db = new SinExWebApp20256461Context();

        public bool ValidateCard(string cardNumber, string cardType)
        {
            bool isValid = false;
            var prefix2 = cardNumber.Substring(0, 2);
            switch (cardType)
            {
                case "American Express":
                    if ((prefix2 == "37" || prefix2 == "34") && cardNumber.Length == 15)
                        isValid = true;
                    break;
                case "Diners Club":
                    string[] prefixValid = { "300", "301", "302", "303", "304", "305", "309" };
                    if ((prefix2 == "55" || prefix2 == "54") && cardNumber.Length == 16)
                        isValid = true;
                    else if ((prefix2 == "38" || prefix2 == "39" || prefix2 == "36") && cardNumber.Length == 14)
                        isValid = true;
                    else if (prefixValid.Contains(cardNumber.Substring(0, 3)) && cardNumber.Length == 14)
                        isValid = true;
                    break;
                case "Discover":
                    if (cardNumber[0] == '6' && (cardNumber.Length == 16 || cardNumber.Length == 19))
                        isValid = true;
                    break;
                case "MasterCard":
                    // 2221 - 2720
                    string[] prefix = { "51", "52", "53", "54", "55" };
                    var prefix4 = Int32.Parse(cardNumber.Substring(0, 4));
                    if ((prefix4 >= 2221 && prefix4 <= 2720 || prefix.Contains(prefix2)) && cardNumber.Length == 16)
                        isValid = true;
                    break;
                case "UnionPay":
                    if (prefix2 == "62" && cardNumber.Length >= 16 && cardNumber.Length <= 19)
                        isValid = true;
                    break;
                case "Visa":
                    int[] validLength = { 13, 16, 19 };
                    if (cardNumber[0] == '4' && validLength.Contains(cardNumber.Length))
                        isValid = true;
                    break;
            }
            return isValid;
        }

        public decimal ConvertCurrency(string currency, decimal value)
        {
            if (Session[currency] == null)
            {
                Session[currency] = db.Currencies.FirstOrDefault(s => s.CurrencyCode == currency).ExchangeRate;

            }
            return decimal.Parse(Session[currency].ToString()) * value;
        }
        public Dictionary<string, decimal> Calculate(string ServiceType, string PackageTypeSize, decimal weight)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();
            ServiceType CostServiceType = db.ServiceTypes.FirstOrDefault(a => a.Type == ServiceType);
            PakageTypeSize CostPackageTypeSize = db.PakageTypeSizes.FirstOrDefault(a => a.size == PackageTypeSize);
            ServicePackageFee CostFee = db.ServicePackageFees.FirstOrDefault(a => a.PackageTypeID == CostPackageTypeSize.PackageType.PackageTypeID && a.ServiceTypeID == CostServiceType.ServiceTypeID);
            decimal penaltyFee = db.PenaltyFees.FirstOrDefault(a => a.PenaltyFeeID == 1).Fee;
            string WeightLimit = CostPackageTypeSize.weightLimit;
            decimal Fee = CostFee.Fee;
            decimal MinimumFee = CostFee.MinimumFee;
            decimal Price = Fee;
            if (CostPackageTypeSize.PackageType.Type == "Envelope")
            {
                Price = Fee;
            }
            else
            {
                decimal actualWeight = Math.Round(weight, 1);
                Price = Fee * actualWeight;
                if (CostPackageTypeSize.PackageType.Type != "Tube" && CostPackageTypeSize.PackageType.Type != "Customer")
                {
                    decimal actualWeightLimit = decimal.Parse(WeightLimit.Replace("kg", ""));
                    if (actualWeight > actualWeightLimit)
                    {
                        Price += penaltyFee;
                    }
                }
                if (Price < MinimumFee)
                {
                    Price = MinimumFee;
                }
            }
            result.Add("CNY", Price);
            result.Add("HKD", ConvertCurrency("HKD", Price));
            result.Add("MOP", ConvertCurrency("MOP", Price));
            result.Add("TWD", ConvertCurrency("TWD", Price));
            return result;
        }
        public bool ValidCity(string city)
        {
            List<string> cities = db.Destinations.Select(a => a.City).ToList();
            if (cities.Contains(city))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public  bool CityMatchProCode(string city, string provinceCode)
        {
            if (this.ValidCity(city))
            {
                string PCode = db.Destinations.SingleOrDefault(a => a.City == city).ProvinceCode;
                if (PCode == provinceCode)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool isTodayOrLater(DateTime expireDate)
        {
            DateTime today = DateTime.Now.Date;
            if(expireDate < today)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public decimal getConvertedCost(double rate, decimal value)
        {
            decimal decimalExchangeRate = decimal.Parse(rate.ToString());
            decimal result = decimalExchangeRate * value;
            return result;
        }

        public bool validateProvinceCode(string code)
        {
            string[] validCodes = { "BJ", "JL", "HN", "SC", "CQ", "JX",
                "QH", "GD", "GZ", "HI", "NM", "ZJ", "HL", "AH", "NM", "HK",
                "NM", "SD", "XJ", "YN", "GS", "XZ", "MC", "JX", "JS", "JX",
                "HL", "SH", "LN", "HE", "TW", "SX", "HE", "XJ", "HB", "SN",
                "QH", "NX", "GS", "HA" };
            if (validCodes.Contains(code))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public IQueryable sortByParam(string sortOrder, IQueryable<ShipmentsListViewModel> shipmentQuery)
        {
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
            return shipmentQuery;
        }
        public ActionResult CheckRecipient(CreateShipmentViewModel shipmentView = null)
        {
            if (shipmentView.ShipmentPayer == "Recipient" || shipmentView.TaxPayer == "Recipient")
            {
                if (string.IsNullOrWhiteSpace(shipmentView.RecipientShippingAccountNumber))
                {
                    ViewBag.errorMessage = "Recipient's Shipping Account Number is required once selected as payer";
                    return View(shipmentView);
                }
            }
            return RedirectToAction("Index");
        }
        public ActionResult ManagePackage(string submit, CreateShipmentViewModel shipmentView = null)
        {
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
                if (shipmentView.Packages.Count > 1)
                {
                    int id = Int32.Parse(submit.Split(' ')[1]) - 1;
                    shipmentView.Packages.Remove(shipmentView.Packages[id]);
                }
                return View(shipmentView);
            }
            return RedirectToAction("Index");
        }
    }
}