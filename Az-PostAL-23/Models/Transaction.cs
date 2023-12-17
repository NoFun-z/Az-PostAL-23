using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Az_PostAL_23.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public double Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string? Uri { get; set; }

        public string? UriName { get; set; }

        [Column(TypeName = "nvarchar(25)")]
        public string? Status { get; set; } = "Idle";

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Title;
            }
        }

        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? "- " : "+ ") + Amount.ToString("C0");
            }
        }
    }
}
