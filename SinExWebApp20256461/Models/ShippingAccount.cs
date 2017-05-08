using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    [Table("ShippingAccount")]
    public abstract class ShippingAccount
    {
        // TODO: how to auto generate 12-digit shipping account
        public virtual int ShippingAccountId { get; set; }
        public virtual string ShippingAccountNumber { get; set; }
        [Display(Name = "Username")]
        [StringLength(10)]
        public string UserName { get; set; }
        public virtual ICollection<Shipment> Shipments { get;set; }
        // for mailing address----------------------------
        [Display(Name = "Building")]
        [StringLength(50)]
        public virtual string BuildingInformation { set; get; }
        [Display(Name = "Street")]
        [Required]
        [StringLength(35)]
        public virtual string StreetInformation { set; get; }
        [Required]
        [StringLength(25)]
        public virtual string City { set; get; }
        [Display(Name = "Province Code")]
        [Required]
        [StringLength(2, MinimumLength =2)]
        [RegularExpression(@"^BJ|JL|HN|SC|CQ|JX|QH|GD|GZ|HI|NM|ZJ|HL|AH|NM|HK|NM|SD|XJ|YN|GS|XZ|MC|JX|JS|JX|HL|SH|LN|HE|TW|SX|HE|XJ|HB|SN|QH|NX|GS|HA$",
            ErrorMessage = "Please input valid Code")]
        public virtual string ProvinceCode { set; get; }
        [Display(Name = "Postal Code")]
        [StringLength(6, MinimumLength = 5)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please input number between 5-6 digits")]
        public virtual string PostalCode { get; set; }
        // --------mailing address over -------------

        //--- credit card information ---------------
        [Display(Name = "Card Type")]
        [Required]
        [RegularExpression(@"^American Express|Diners Club|Discover|MasterCard|UnionPay|Visa$", ErrorMessage = "Only the following types are allowed: American Express, Diners Club, Discover, MasterCard, UnionPay, Visa")]
        public virtual string CardType { get; set; }
        [Display(Name = "Card Number")]
        [Required]
       //  [StringLength(19, MinimumLength = 14)]
        [RegularExpression(@"^[0-9]{14,19}$", ErrorMessage = "Please input number between 14-19 digits")]
        public virtual string CardNumber { get; set; }
        [Display(Name = "Security Number")]
        [Required]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "security # should be number and between 3-4 digits")]
        public virtual string SecurityNumber { get; set; }
        [Display(Name = "Card Holder Name")]
        [Required]
        [StringLength(70)]
        public virtual string CardHolderName { get; set; }

        [Display(Name = "Expiry Month")]
        [Required]
        [RegularExpression(@"^(0?[1-9]|1[012])$", ErrorMessage = "Please input valid month")]
        public virtual string Month { get; set; }

        [Display(Name = "Expiry Year")]
        [Required]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^20[1-3][0-9]*$", ErrorMessage ="Please input valid year")]
        public virtual string Year { get; set; }

        [Display(Name = "Phone Number")]
        [Required]
        [StringLength(14, MinimumLength = 8)]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Phone number must contsains 8-14 digits.")]
        public virtual string PhoneNumber { get; set; }
        [Display(Name = "Email Address")]
        [Required]
        [StringLength(30)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public virtual string EmailAddress { get; set; }

        public virtual int SavedAddressID { get; set; }
        public virtual ICollection<SavedAddress> SavedAddresses { get; set; }
        // public virtual ICollection<Invoice> Invoices { get; set; }
    }
}