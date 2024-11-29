using RefactorThis.Domain.Core;
using RefactorThis.Domain.Enums;
using RefactorThis.Persistence;
using System;
using System.Linq;

namespace RefactorThis.Application
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;
        private const decimal TaxRate = 0.14m;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference)
                            ?? throw new InvalidOperationException("There is no invoice matching this payment.");

            if (invoice.Amount == 0)
                return HandleZeroAmountInvoice(invoice);

            if (invoice.Payments != null && invoice.Payments.Any())
                return HandleExistingPayments(invoice, payment);

            return HandleNewPayment(invoice, payment);

            //var inv = _invoiceRepository.GetInvoice(payment.Reference);

            //var responseMessage = string.Empty;

            //if (inv.Payments != null && inv.Payments.Any())
            //{ //HandleExistingPayments
            //         //totalPaid
            //    if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.Payments.Sum(x => x.Amount))
            //    {
            //        responseMessage = "invoice was already fully paid";
            //    }          //totalPaid
            //    else if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
            //    {
            //        responseMessage = "the payment is greater than the partial amount remaining";
            //    }  //HandleExistingPayments
            //    else
            //    { //ProcessPartialOrFinalPayment
            //        if ((inv.Amount - inv.AmountPaid) == payment.Amount)
            //        {
            //            switch (inv.Type)
            //            {
            //                case InvoiceType.Standard:
            //                    inv.AmountPaid += payment.Amount;
            //                    inv.Payments.Add(payment);
            //                    responseMessage = "final partial payment received, invoice is now fully paid";
            //                    break;
            //                case InvoiceType.Commercial:
            //                    inv.AmountPaid += payment.Amount;
            //                    inv.TaxAmount += payment.Amount * 0.14m;
            //                    inv.Payments.Add(payment);
            //                    responseMessage = "final partial payment received, invoice is now fully paid";
            //                    break;
            //                default:
            //                    throw new ArgumentOutOfRangeException();
            //            }
            //        }
            //        else
            //        {//ProcessPartialPayment
            //            switch (inv.Type)
            //            {
            //                case InvoiceType.Standard:
            //                    inv.AmountPaid += payment.Amount;
            //                    inv.Payments.Add(payment);
            //                    responseMessage = "another partial payment received, still not fully paid";
            //                    break;
            //                case InvoiceType.Commercial:
            //                    inv.AmountPaid += payment.Amount;
            //                    inv.TaxAmount += payment.Amount * 0.14m;
            //                    inv.Payments.Add(payment);
            //                    responseMessage = "another partial payment received, still not fully paid";
            //                    break;
            //                default:
            //                    throw new ArgumentOutOfRangeException();
            //            }
            //        }
            //    }
            //}
            //else
            //{// HandleNewPayment
            //    if (payment.Amount > inv.Amount)
            //    {
            //        responseMessage = "the payment is greater than the invoice amount";
            //    }
            //    else if (inv.Amount == payment.Amount)
            //    {//ProcessFinalPayment
            //        switch (inv.Type)
            //        {
            //            case InvoiceType.Standard:
            //                inv.AmountPaid = payment.Amount;
            //                inv.TaxAmount = payment.Amount * 0.14m;
            //                inv.Payments.Add(payment);
            //                responseMessage = "invoice is now fully paid";
            //                break;
            //            case InvoiceType.Commercial:
            //                inv.AmountPaid = payment.Amount;
            //                inv.TaxAmount = payment.Amount * 0.14m;
            //                inv.Payments.Add(payment);
            //                responseMessage = "invoice is now fully paid";
            //                break;
            //            default:
            //                throw new ArgumentOutOfRangeException();
            //        }
            //    }
            //    else
            //    {//ProcessPartialPayment
            //        switch (inv.Type)
            //        {
            //            case InvoiceType.Standard:
            //                inv.AmountPaid = payment.Amount;
            //                inv.TaxAmount = payment.Amount * 0.14m;
            //                inv.Payments.Add(payment);
            //                responseMessage = "invoice is now partially paid";
            //                break;
            //            case InvoiceType.Commercial:
            //                inv.AmountPaid = payment.Amount;
            //                inv.TaxAmount = payment.Amount * 0.14m;
            //                inv.Payments.Add(payment);
            //                responseMessage = "invoice is now partially paid";
            //                break;
            //            default:
            //                throw new ArgumentOutOfRangeException();
            //        }
            //    }
            //}
            //_invoiceRepository.SaveInvoice(inv);

            //return responseMessage;
        }

        private string HandleZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
            {
                return "No payment needed.";
            }

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private string HandleExistingPayments(Invoice invoice, Payment payment)
        {
            var totalPaid = invoice.Payments.Sum(x => x.Amount);

            if (totalPaid != 0)
            {
                if (totalPaid == invoice.Amount)
                {
                    return "Invoice was already fully paid.";
                }

                if (payment.Amount > (invoice.Amount - invoice.AmountPaid))
                {
                    return "The payment is greater than the partial amount remaining.";
                }
            }

            return ProcessPartialOrFinalPayment(invoice, payment);
        }

        private string ProcessPartialOrFinalPayment(Invoice invoice, Payment payment)
        {
            if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
            {
                return ProcessFinalPayment(invoice, payment);
            }

            return ProcessPartialPayment(invoice, payment,false);
        }

        private string ProcessFinalPayment(Invoice invoice, Payment payment)
        {
            UpdateInvoice(invoice, payment, true);
            if (invoice.AmountPaid == 0 && invoice.Amount == payment.Amount)
            {
                return "Invoice is now fully paid.";
            }

            return "Final partial payment received, invoice is now fully paid.";
        }

        private string ProcessPartialPayment(Invoice invoice, Payment payment, bool isInitialPayment)
        {
            UpdateInvoice(invoice, payment, false);
            if (isInitialPayment)
            {
                return "Invoice is now partially paid.";
            }
            return "Another partial payment received, still not fully paid.";
        }

        private void UpdateInvoice(Invoice invoice, Payment payment, bool isFinalPayment)
        {
            invoice.AmountPaid += payment.Amount;

            if (invoice.Type == InvoiceType.Commercial)
            {
                invoice.TaxAmount += payment.Amount * TaxRate; //0.14m;
            }

            invoice.Payments.Add(payment);

            if (isFinalPayment && invoice.Amount == invoice.AmountPaid)
            {
                invoice.IsFullyPaid = true; // Assuming `IsFullyPaid` is a field to mark the invoice as fully paid.
            }

            _invoiceRepository.SaveInvoice(invoice);
        }

        private string HandleNewPayment(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
            {
                return "The payment is greater than the invoice amount.";
            }

            if (payment.Amount == invoice.Amount)
            {
                return ProcessFinalPayment(invoice, payment);
            }

            return ProcessPartialPayment(invoice, payment, true);
        }

    }
}
