using RefactorThis.Domain.Core;
using System.Threading.Tasks;

namespace RefactorThis.Persistence
{
    /// <summary>
    /// Repository for managing invoice persistence operations.
    /// </summary>
    public interface IInvoiceRepository
    {
        /// <summary>
        /// Creates a new invoice record in the database.
        /// </summary>
        /// <param name="invoice">The invoice to be added.</param>
        /// <returns>The ID of the newly created invoice.</returns>
        Task<int> CreateInvoiceAsync(Invoice invoice);

        /// <summary>
        /// Updates an existing invoice to the database.
        /// </summary>
        /// <param name="invoice">The invoice to be updated.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateInvoiceAsync(Invoice invoice);

        /// <summary>
        /// Retrieves an invoice by its reference.
        /// </summary>
        /// <param name="reference">The reference identifier of the invoice.</param>
        /// <returns>The invoice with the specified reference, or null if not found.</returns>
        Task<Invoice> GetInvoiceAsync(string reference);
    }
}