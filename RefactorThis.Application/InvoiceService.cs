using RefactorThis.Domain.Core;
using RefactorThis.Domain.Enums;
using RefactorThis.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RefactorThis.Application
{
    public class InvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private const decimal TaxRate = 0.14m;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<string> ProcessPaymentAsync(Payment payment)
        {
            var invoice = await _invoiceRepository.GetInvoiceAsync(payment.Reference)
                            ?? throw new InvalidOperationException("There is no invoice matching this payment.");

            if (invoice.Amount == 0)
                return HandleZeroAmountInvoice(invoice);

            if (invoice.Payments != null && invoice.Payments.Any())
                return await HandleExistingPaymentsAsync(invoice, payment);

            return await HandleNewPaymentAsync(invoice, payment);
        }

        private string HandleZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
                return "No payment needed.";

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private async Task<string> HandleExistingPaymentsAsync(Invoice invoice, Payment payment)
        {
            var totalPaid = invoice.Payments.Sum(x => x.Amount);

            if (totalPaid != 0)
            {
                if (totalPaid == invoice.Amount)
                    return "Invoice was already fully paid.";

                if (payment.Amount > (invoice.Amount - invoice.AmountPaid))
                    return "The payment is greater than the partial amount remaining.";
            }

            return await ProcessPartialOrFinalPaymentAsync(invoice, payment);
        }

        private async Task<string> ProcessPartialOrFinalPaymentAsync(Invoice invoice, Payment payment)
        {
            if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
                return await ProcessFinalPaymentAsync(invoice, payment);

            return await ProcessPartialPaymentAsync(invoice, payment, false);
        }

        private async Task<string> ProcessFinalPaymentAsync(Invoice invoice, Payment payment)
        {
            await UpdateInvoicePaymentAsync(invoice, payment);
            return "Final partial payment received, invoice is now fully paid.";
        }

        private async Task<string> ProcessPartialPaymentAsync(Invoice invoice, Payment payment, bool isInitialPayment)
        {
            await UpdateInvoicePaymentAsync(invoice, payment);
            if (isInitialPayment)
                return "Invoice is now partially paid.";

            return "Another partial payment received, still not fully paid.";
        }

        private async Task UpdateInvoicePaymentAsync(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;

            if (invoice.Type == InvoiceType.Commercial)
                invoice.TaxAmount += payment.Amount * TaxRate;

            invoice.Payments.Add(payment);

            await _invoiceRepository.UpdateInvoiceAsync(invoice);
        }

        private async Task<string> HandleNewPaymentAsync(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
                return "The payment is greater than the invoice amount.";

            if (payment.Amount == invoice.Amount)
                return await ProcessFinalPaymentAsync(invoice, payment);

            return await ProcessPartialPaymentAsync(invoice, payment, true);
        }
    }
}