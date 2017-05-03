using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

using SinExWebApp20256461.Models;

namespace SinExWebApp20256461.ViewModels
{
    public class CreateShipmentViewModel
    {
        public virtual ShippingAccount ShippingAccount { get; set; }
        public virtual Shipment Shipment { get; set; }
        public virtual Recipient Recipient { get; set; }
        public virtual Pickup Pickup { get; set; }
        public virtual string Status { get; set; }
        public virtual string Nickname { get; set; }
        public virtual string IfSendEmail { get; set; }
        public virtual string ShipmentPayer { get; set; }
        public virtual string TaxPayer { get; set; }
        public virtual string RecipientShippingAccountNumber { get; set; }  // if any
        public virtual IList<Package> Packages { get; set; }
        public virtual List<String> ServiceTypes { get; set; }
        public virtual List<String> PackageTypeSizes { get; set; }
        public virtual double DutyAmount { get; set; }
        public virtual double TaxAmount { get; set; }
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input 4-digit authorization code")]
        public virtual string ShipmentAuthorizationCode { get; set; }
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input 4-digit authorization code")]
        public virtual string DutyAndTaxAuthorizationCode { get; set; }
    }
}