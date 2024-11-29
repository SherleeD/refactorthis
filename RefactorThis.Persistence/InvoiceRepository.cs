using RefactorThis.Domain.Core;
using System;
using System.Threading.Tasks;

namespace RefactorThis.Persistence
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Task<int> CreateInvoiceAsync(Invoice invoice)
        {
            _invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));

            return Task.FromResult(invoice.Id); 
        }

        public Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            _invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));
            return Task.FromResult(true);
        }

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public Task<Invoice> GetInvoiceAsync(string reference)
        {
         
            return Task.FromResult(_invoice);
        }
    }
}