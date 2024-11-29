using RefactorThis.Domain.Enums;
using System.Collections.Generic;

namespace RefactorThis.Domain.Core
{
    public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public InvoiceType Type { get; set; }
        public bool IsFullyPaid { get; set; }
    }
}