using NUnit.Framework;
using RefactorThis.Application;
using RefactorThis.Domain.Core;
using RefactorThis.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        [Test]
        public async Task ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReferenceAsync()
        {
            var repo = new InvoiceRepository();
            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();
            var failureMessage = "";

            try
            {
                var result = await paymentProcessor.ProcessPaymentAsync(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.AreEqual("There is no invoice matching this payment.", failureMessage);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeededAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("No payment needed.", result);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaidAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 10
                    }
                }
            };
            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("Invoice was already fully paid.", result);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDueAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                }
            };
            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment()
            {
                Amount = 6,
                Reference = "PAYREF01"
            };
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("The payment is greater than the partial amount remaining.", result);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmountAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment()
            {
                Amount = 6,
                Reference = "PAYREF02"
            };
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("The payment is greater than the invoice amount.", result);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDueAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                }
            };
            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment()
            {
                Amount = 5,
                Reference = "PAYREF03"
            };
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("Final partial payment received, invoice is now fully paid.", result);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmountAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment() { Amount = 10, Reference = "PAYREF04" } }
            };
            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment()
            {
                Amount = 10,
                Reference = "PAYREF05"
            };
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("Invoice was already fully paid.", result);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDueAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                }
            };
            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment()
            {
                Amount = 1,
                Reference = "PAYREF06"
            };
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("Another partial payment received, still not fully paid.", result);
        }

        [Test]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmountAsync()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = Enums.InvoiceType.Commercial
            };
            invoice.Id = await repo.CreateInvoiceAsync(invoice);

            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment()
            {
                Amount = 1,
                Reference = "PAYREF07"
            };
            var result = await paymentProcessor.ProcessPaymentAsync(payment);

            Assert.AreEqual("Invoice is now partially paid.", result);
        }
    }
}