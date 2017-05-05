using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    public class Invoice
    {
        public virtual int InvoiceID {get;set;}
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input a 4-digit number")]
        [Display(Name = "Authorization Code")]
        public virtual string AuthenticationCode { get; set; }
        public virtual string Type { get; set; }
        // on the diagram, says Account Number
        // public virtual int ShippingAccountId { get; set; }
        // public virtual ShippingAccount ShippingAccount { get; set; }
        [Display(Name = "Shipping Account Number")]
        public virtual string ShippingAccountNumber { get; set; }
        [Display(Name = "Total Amount Payable")]
        public virtual double TotalAmountPayable { get; set; }  // if the type is "shipment"
        public virtual double Duty { get; set; }    // if the type is "duty&tax"; notice that they r seperate
        public virtual double Tax { get; set; }
        public virtual int WaybillId { get; set; }
        public virtual Shipment Shipment { get; set; }
    }
}