using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    public class SavedAddress
    {
        public virtual int SavedAddressID { get; set; }
        public virtual string NickName { get; set; }
        /*newly added attributes*/
        [StringLength(50)]
        public virtual string Building { get; set; }  //optional
        [StringLength(35)]
        public virtual string Street { get; set; }
        [StringLength(25)]
        public virtual string City { get; set; }
        [StringLength(2, MinimumLength = 2)]
        [RegularExpression(@"^BJ|JL|HN|SC|CQ|JX|QH|GD|GZ|HI|NM|ZJ|HL|AH|NM|HK|NM|SD|XJ|YN|GS|XZ|MC|JX|JS|JX|HL|SH|LN|HE|TW|SX|HE|XJ|HB|SN|QH|NX|GS|HA$",
    ErrorMessage = "Please input valid Code")]
        public virtual string ProvinceCode { set; get; }
        [StringLength(6, MinimumLength = 5)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input number between 5-6 digits")]
        public virtual string PostalCode { get; set; }
        public virtual string PickupLocation { get; set; }
        public virtual string Type { get; set; }
        public virtual int ShippingAccountId { get; set; }
        public virtual ShippingAccount ShippingAccount { get; set; }

    }
}