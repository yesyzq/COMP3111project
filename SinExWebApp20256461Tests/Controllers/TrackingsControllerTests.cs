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
    public class TrackingsControllerTests
    {
        TrackingsController myController;
        [OneTimeSetUp]
        public void TestSetUp()
        {
            myController = new TrackingsController();
        }
        [Test()]
        public void CreateTest()
        {
            ViewResult result = myController.Create() as ViewResult;
            Assert.IsNotNull(result);
        }
    }
}