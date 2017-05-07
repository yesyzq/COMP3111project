using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SinExWebApp20256461.Models
{
    [Table("PenaltyFee")]
    public class PenaltyFee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int PenaltyFeeID { get; set; }
        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "{0} must be a number between {1} and {2}.")]
        public virtual decimal Fee { get; set; }
    }
}