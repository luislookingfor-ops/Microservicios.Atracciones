namespace Microservicios.Atracciones.Billing.DataAccess.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPaymentRepository Payments { get; }
    IInvoiceRepository Invoices { get; }
    IInvoiceDetailRepository InvoiceDetails { get; }

    Task<int> CompleteAsync();
    Task ExecuteSqlRawAsync(string sql, params object[] parameters);
}
