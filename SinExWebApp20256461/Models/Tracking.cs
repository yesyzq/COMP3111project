using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    public class Tracking
    {
        [key]
        public virtual int TrackingID { get; set; }
        public virtual int WaybillId { get; set; }
        [Display(Name = "Waybill Number")]
        public virtual string WaybillNumber { get; set; }
        public virtual Shipment Shipment { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual string Description { get; set; }
        public virtual string Location { get; set; }
        public virtual string Remarks { get; set; }                                                                     
    }
}