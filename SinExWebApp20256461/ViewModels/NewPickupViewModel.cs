﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SinExWebApp20256461.Models;

namespace SinExWebApp20256461.ViewModels
{
    public class NewPickupViewModel
    {
        public virtual IEnumerable<String> Locations { get; set; }
        public virtual Pickup Pickup { get; set; }
        public virtual string PickupNickname { get; set; }
        public virtual string RecipientNickname { get; set; }
        public virtual int shipmentID { get; set; }

    }
}