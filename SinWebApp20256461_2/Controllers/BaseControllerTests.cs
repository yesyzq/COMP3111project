using NUnit.Framework;
using SinExWebApp20256461.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

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

        //[Test()]
        //public void ConvertCurrencyTest()
        //{
        //    Assert.Fail();
        //}

        //[Test()]
        //public void CalculateTest()
        //{
        //    Assert.Fail();
        //}

        //[Test()]
        //public void ValidCityTest()
        //{
        //    Assert.Fail();
        //}

        //[TestCase("Beijing", "BJ", true)]
        //[TestCase("Beijing", "SH", false)]
        //public void CityMatchProCodeTest(string city, string provinceCode, bool result)
        //{
        //    Assert.That(myController.CityMatchProCode(city, provinceCode), Is.EqualTo(result));
        //}
    }
}