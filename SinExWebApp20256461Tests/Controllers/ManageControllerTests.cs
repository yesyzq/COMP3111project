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
    public class ManageControllerTests
    {
        ManageController myController;
        [OneTimeSetUp]
        public void TestSetUp()
        {
            myController = new ManageController();
        }
        [Test()]
        public void ChangePasswordTest()
        {
            ViewResult result = myController.ChangePassword() as ViewResult;
            Assert.IsNotNull(result);
        }

        [Test()]
        public void SetPasswordTest()
        {
            ViewResult result = myController.SetPassword() as ViewResult;
            Assert.IsNotNull(result);
        }
    }
}