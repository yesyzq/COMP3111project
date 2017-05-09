using NUnit.Framework;
using SinExWebApp20256461.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using SinExWebApp20256461.Models;
using SinExWebApp20256461.ViewModels;


namespace SinExWebApp20256461.Controllers.Tests
{
    [TestFixture()]
    public class BaseControllerTests
    {
        BaseController myController;

        [OneTimeSetUp]
        public void TestSetUp()
        {
            myController = new BaseController();
        }

        // Credit Card Validation
        [TestCase("5408888888888888", "MasterCard", true)]
        [TestCase("5008888888888888", "MasterCard", false)]
        [TestCase("371234569874563", "American Express", true)]
        [TestCase("271234569874563", "American Express", false)]
        [TestCase("5512345678987654", "Diners Club", true)]
        [TestCase("3101234567898765", "Diners Club", false)]
        [TestCase("6111111111111111", "Discover", true)]
        [TestCase("5111111111111111", "Discover", false)]
        [TestCase("6200000000000000", "UnionPay", true)]
        [TestCase("6111111111111111", "UnionPay", false)]
        [TestCase("4000000022112211", "Visa", true)]
        [TestCase("400000002211221", "Visa", false)]
        public void ValidateCardTest(string cardNumber, string cardType, bool result)
        {
            Assert.That(myController.ValidateCard(cardNumber, cardType), Is.EqualTo(result));
        }

        [TestCase("5/8/2017", false)]
        [TestCase("7/8/2017", true)]
        public void isTodayOrLaterTest(DateTime expireDate, bool result)
        {
            Assert.That(myController.isTodayOrLater(expireDate), Is.EqualTo(result));
        }

        [TestCase(1.13, 2, 2.26)]
        [TestCase(4.56, 10, 45.6)]
        [TestCase(1.16, 2.2222, 2.577752)]
        public void getConvertedCostTest(double rate, decimal value, decimal result)
        {
            Assert.That(myController.getConvertedCost(rate, value), Is.EqualTo(result));
        }

        [TestCase("BJ", true)]
        [TestCase("HK", true)]
        [TestCase("MC", true)]
        [TestCase("TW", true)]
        [TestCase("GG", false)]
        public void validateProvinceCodeTest(string code, bool result)
        {
            Assert.That(myController.validateProvinceCode(code), Is.EqualTo(result));
        }

        /* test all views that are returned by Shipments/Create[POST] */
        [TestCase]
        public void AddPackageInvalid()
        {
            var shipmentView = new CreateShipmentViewModel();
            IList<Package> packages = new List<Package>(new Package[10]);
            shipmentView.Packages = packages;
            string submit = "add";
            var result = myController.ManagePackage(submit, shipmentView) as ViewResult;
            var result_shipment = (CreateShipmentViewModel)result.ViewData.Model;
            Assert.AreEqual(shipmentView, result_shipment);
        }

        [TestCase]
        public void AddPackageValid()
        {
            var shipmentView = new CreateShipmentViewModel();
            IList<Package> packages = new List<Package>(new Package[8]);
            shipmentView.Packages = packages;
            string submit = "add";
            var result = myController.ManagePackage(submit, shipmentView) as ViewResult;
            var result_shipment = (CreateShipmentViewModel)result.ViewData.Model;
            Assert.AreEqual(9, result_shipment.Packages.Count);
        }

        [TestCase]
        public void DeletePackageValid()
        {
            var shipmentView = new CreateShipmentViewModel();
            IList<Package> packages = new List<Package>(new Package[10]);
            shipmentView.Packages = packages;
            string submit = "delete 5";
            var result = myController.ManagePackage(submit, shipmentView) as ViewResult;
            var result_shipment = (CreateShipmentViewModel)result.ViewData.Model;
            Assert.AreEqual(9, result_shipment.Packages.Count);
        }

        [TestCase]
        public void DeletePackageInvalid()
        {
            var shipmentView = new CreateShipmentViewModel();
            IList<Package> packages = new List<Package>(new Package[10]);
            shipmentView.Packages = packages;
            string submit = "delete 1";
            var result = myController.ManagePackage(submit, shipmentView) as ViewResult;
            var result_shipment = (CreateShipmentViewModel)result.ViewData.Model;
            Assert.AreEqual(shipmentView, result_shipment);
        }

        [TestCase]
        public void CheckRecipientNone()
        {
            var shipmentView = new CreateShipmentViewModel();
            shipmentView.ShipmentPayer = "Recipient";
            var result = myController.CheckRecipient(shipmentView) as ViewResult;
            var errorMessage = result.ViewData["errorMessage"];
            var valid_result = "Recipient's Shipping Account Number is required once selected as payer";
            Assert.AreEqual(valid_result, errorMessage);
        }

        [TestCase("DeliveredDate")]
        [TestCase("ShippedDate")]
        [TestCase("DeliveredDate_desc")]
        [TestCase("ShippedDate_desc")]
        public void CheckQuerySort(string sortOrder)
        {
            IQueryable<ShipmentsListViewModel> ret = QuerySortByParam(sortOrder);
            switch (sortOrder)
            {
                case "DeliveredDate":
                    Assert.AreEqual(true, ret.First().DeliveredDate < ret.Skip(1).First().DeliveredDate);
                    break;
                case "ShippedDate":
                    Assert.AreEqual(true, ret.First().ShippedDate < ret.Skip(1).First().ShippedDate);
                    break;
                case "DeliveredDate_desc":
                    Assert.AreEqual(true, ret.First().DeliveredDate > ret.Skip(1).First().DeliveredDate);
                    break;
                case "ShippedDate_desc":
                    Assert.AreEqual(true, ret.First().ShippedDate > ret.Skip(1).First().ShippedDate);
                    break;
            }
        }

        public IQueryable<ShipmentsListViewModel> QuerySortByParam(string sortOrder)
        {
            List<ShipmentsListViewModel> data = new List<ShipmentsListViewModel> {
                new ShipmentsListViewModel
                {
                    WaybillNumber = "0000000000000001",
                    ServiceType = "",
                    ShippedDate = new DateTime(2017, 3, 9, 16, 5, 7, 123),
                    DeliveredDate = new DateTime(2017, 5, 9, 16, 5, 7, 123),
                    RecipientName = "",
                    NumberOfPackages = 2,
                    Origin = "BJ",
                    Destination = "SH",
                    ShippingAccountNumber = "000000000001",
                    Status = "pending"
                },
                new ShipmentsListViewModel
                {
                    WaybillNumber = "0000000000000002",
                    ServiceType = "",
                    ShippedDate = new DateTime(2017, 4, 9, 16, 5, 7, 123),
                    DeliveredDate = new DateTime(2017, 6, 9, 16, 5, 7, 123),
                    RecipientName = "",
                    NumberOfPackages = 5,
                    Origin = "SH",
                    Destination = "BJ",
                    ShippingAccountNumber = "000000000002",
                    Status = "confirmed"
                }
            };
            var query = data.AsQueryable();
            var new_query = myController.sortByParam(sortOrder, query);
            return (IQueryable<ShipmentsListViewModel>)new_query;
        }
    }
}