using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    [Table("ServiceType")]
    public class ServiceType
    {
        public virtual int ServiceTypeID { get; set; }
        public virtual string Type { get; set; }
        [Display(Name = "Cutoff Time")]
        public virtual string CutoffTime { get; set; }
        [Display(Name = "Delivery Time")]
        public virtual string DeliveryTime { get; set; }
        public virtual ICollection<ServicePackageFee> ServicePackageFees { get; set; }
    }
}