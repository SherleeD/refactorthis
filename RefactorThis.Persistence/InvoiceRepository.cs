using RefactorThis.Domain.Core;
using System;

namespace RefactorThis.Persistence
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            _invoice = invoice;
        }

        public void AddInvoice(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            _invoice = invoice;
        }
    }
}