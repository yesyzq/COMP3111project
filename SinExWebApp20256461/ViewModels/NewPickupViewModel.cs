using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SinExWebApp20256461.Models;
using System.ComponentModel.DataAnnotations;

namespace SinExWebApp20256461.ViewModels
{
    public class NewPickupViewModel
    {
        public virtual Pickup Pickup { get; set; }
        [Display(Name = "Pickup Nickname")]
        public virtual string PickupNickname { get; set; }
        [Display(Name = "Recipient Nickname")]
        public virtual string RecipientNickname { get; set; }
        [Display(Name = "Pickup Location Nickname")]
        public virtual String PickupLocationNickname { get; set; }
        //For now, we implement 1..1 relationship between Shipment and Pickup
        public int WaybillId { get; set; }

        public virtual string locationIsSame { get; set; }
    }
}