using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.ViewModels
{
    public class InvoicesListViewModel
    {
        [Display(Name = "Waybill Number")]
        public virtual string WaybillNumber { get; set; }
        [Display(Name = "Service Type")]
        public virtual string ServiceType { get; set; }
        [Display(Name = "Shipped Date")]
        public virtual DateTime ShippedDate { get; set; }
        [Display(Name = "Recipient Name")]
        public virtual string RecipientName { get; set; }
        [Display(Name = "Total Amount Payable")]
        public virtual double TotalAmountPayable { get; set; }
        public virtual string Origin { get; set; }
        public virtual string Destination { get; set; }
        [Display(Name = "Shipping Account Number")]
        public virtual string ShippingAccountNumber { get; set; }
        public virtual string Type { get; set; }
    }
}