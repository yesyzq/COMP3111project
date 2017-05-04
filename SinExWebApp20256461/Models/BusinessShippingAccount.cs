using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    public class BusinessShippingAccount: ShippingAccount
    {
        [Display(Name = "Contact Person Name")]
        [Required]
        [StringLength(70)]
        public virtual string ContactPersonName { set; get; }
        [Display(Name = "Company Name")]
        [Required]
        [StringLength(40)]
        public virtual string CompanyName { set; get; }
        [Display(Name = "Department Name")]
        [Required]
        [StringLength(30)]
        public virtual string DepartmentName { set; get; }
    }
}