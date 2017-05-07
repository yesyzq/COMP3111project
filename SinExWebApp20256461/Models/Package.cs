using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    public class Package
    {
        [key]
        public virtual int PackageID { get; set; }
        public virtual  int WaybillId { get; set; }
        public virtual Shipment Shipment { get; set; }
        [Display(Name = "Shipping Account Number")]
        public virtual string ShippingAccountNumber { get; set; }
        public virtual string Description { get; set; }
        [Range(typeof(double), "0", "999999999999", ErrorMessage = "{0} must be a number between {1} and {2}.")]
        public virtual double Value { get; set; }
        [Required]
        public virtual string Currency { get; set; }
        [Range(typeof(double), "0", "99999", ErrorMessage = "{0} must be a number between {1} and {2}.")]
        [Display(Name = "Estimated Weight")]
        [Required]
        public virtual double WeightEstimated { get; set; }
        [Range(typeof(double), "0", "99999", ErrorMessage = "{0} must be a number between {1} and {2}.")]
        [Display(Name = "Actual Weight")]
        public virtual double WeightActual { get; set; }
        //public virtual int PackageTypeID { get; set; }
        //public virtual PackageType PackageType  { get; set; }
        [Display(Name = "Package Type Size")]
        [Required]
        public virtual string PackageTypeSize { get; set; }    //for easy creation
        //public virtual PakageTypeSize PackageTypeSize { get; set; }
    }
}