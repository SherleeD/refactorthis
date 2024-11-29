using System.ComponentModel;

namespace RefactorThis.Domain.Enums
{
    /// <summary>
    /// List of invoice types
    /// </summary>
    public enum InvoiceType
    {
        [Description("Standard Invoice")]
        Standard,

        [Description("Commercial Invoice")]
        Commercial
    }
}