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
        //public virtual string Status { get; set; }
        public virtual string Nickname { get; set; }
        [Display(Name = "Email Notification?")]
        public virtual string IfSendEmail { get; set; }
        [Display(Name = "Shipment Payer")]
        public virtual string ShipmentPayer { get; set; }
        [Display(Name = "Tax Payer")]
        public virtual string TaxPayer { get; set; }
        [Display(Name = "Recipient Shipping Account Number")]
        public virtual string RecipientShippingAccountNumber { get; set; }  // if any
        public virtual IList<Package> Packages { get; set; }
        public virtual List<String> ServiceTypes { get; set; }
        public virtual List<String> PackageTypeSizes { get; set; }
        [Range(typeof(double), "0", "999999999999", ErrorMessage = "{0} must be a number between {1} and {2}.")]
        [Display(Name = "Duty Amount")]
        public virtual double DutyAmount { get; set; }
        [Range(typeof(double), "0", "999999999999", ErrorMessage = "{0} must be a number between {1} and {2}.")]
        [Display(Name = "Tax Amount")]
        public virtual double TaxAmount { get; set; }
        [Display(Name = "Shipment Authorization Code")]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input 4-digit authorization code")]
        public virtual string ShipmentAuthorizationCode { get; set; }
        [Display(Name = "Duty&Tax Authorization Code")]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input 4-digit authorization code")]
        public virtual string DutyAndTaxAuthorizationCode { get; set; }
        public virtual string IsSavedRecipient { get; set; }
        [Display(Name = "Currency Code")]
        public virtual string TaxCurrency { get; set; }
    }
}