namespace RefactorThis.Domain.Core
{
    /// <summary>
    /// Represents a payment made in the system.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets the amount of the payment. Must be a positive value.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets the reference for the payment. Cannot be null or empty.
        /// </summary>
        public string Reference { get; set; }
    }
}
