using Microservicios.Atracciones.Billing.DataAccess.Context;
using Microservicios.Atracciones.Billing.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicios.Atracciones.Billing.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BillingDbContext _context;

    public UnitOfWork(BillingDbContext context)
    {
        _context = context;
    }

    private IPaymentRepository? _payments;
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);

    private IInvoiceRepository? _invoices;
    public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);

    private IInvoiceDetailRepository? _invoiceDetails;
    public IInvoiceDetailRepository InvoiceDetails => _invoiceDetails ??= new InvoiceDetailRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        await _context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
