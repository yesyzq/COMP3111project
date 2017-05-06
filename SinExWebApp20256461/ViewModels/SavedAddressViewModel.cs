using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SinExWebApp20256461.Models;

namespace SinExWebApp20256461.ViewModels
{
    public class SavedAddressViewModel
    {
        public virtual SavedAddress SavedAddress { get; set; }
        public virtual string WaybillId { get; set; }
        public virtual string PageJumpType { get; set; }
    }
}