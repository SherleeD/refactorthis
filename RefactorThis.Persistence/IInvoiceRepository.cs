using RefactorThis.Domain.Core;

namespace RefactorThis.Persistence
{
    public interface IInvoiceRepository
    {
        void SaveInvoice(Invoice invoice);

        void AddInvoice(Invoice invoice);
    }
}