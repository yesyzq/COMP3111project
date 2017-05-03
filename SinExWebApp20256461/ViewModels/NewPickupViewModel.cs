using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SinExWebApp20256461.Models;

namespace SinExWebApp20256461.ViewModels
{
    public class NewPickupViewModel
    {
        public virtual Pickup Pickup { get; set; }
        public virtual String PickupLocationNickname { get; set; }

        //For now, we implement 1..1 relationship between Shipment and Pickup
        public int WaybillId { get; set; }
    }
}