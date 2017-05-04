using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    public class Recipient
    {
        public virtual int RecipientID { get; set; }
        [Display(Name = "Full Name")]
        [Required]
        [StringLength(509)]
        public virtual string FullName { get; set; }
        [Required]
        [StringLength(14, MinimumLength = 8)]
        [Display(Name = "Phone")]
        [RegularExpression(@"^[0-9 ]*$", ErrorMessage = "Phone number must contsains 8-14 digits.")]
        public virtual string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Email")]
        [StringLength(30)]
        [DataType(DataType.EmailAddress)]
        public virtual string EmailAddress { get; set; }

        /*newly added attributes*/
        [StringLength(50)]
        public virtual string Building { get; set; }  //optional
        [Required]
        [StringLength(35)]
        public virtual string Street { get; set; }
        [Required]
        [StringLength(25)]
        public virtual string City { get; set; }
        [Display(Name = "Province Code")]
        [Required]
        [StringLength(2, MinimumLength = 2)]
        [RegularExpression(@"^BJ|JL|HN|SC|CQ|JX|QH|GD|GZ|HI|NM|ZJ|HL|AH|NM|HK|NM|SD|XJ|YN|GS|XZ|MC|JX|JS|JX|HL|SH|LN|HE|TW|SX|HE|XJ|HB|SN|QH|NX|GS|HA$",
    ErrorMessage = "Please input valid Code")]
        public virtual string ProvinceCode { set; get; }

        [Display(Name = "Postal Code")]
        [StringLength(6, MinimumLength = 5)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input number between 5-6 digits")]
        public virtual string PostalCode { get; set; }
        // Princeple end Problem
        // public virtual int WaybillId { get; set; }
        // public virtual Shipment Shipment { get; set; }
    }
}