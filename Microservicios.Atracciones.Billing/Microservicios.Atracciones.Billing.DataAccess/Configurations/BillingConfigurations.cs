using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microservicios.Atracciones.Billing.DataAccess.Entities;

namespace Microservicios.Atracciones.Billing.DataAccess.Configurations;

public class PaymentMethodTypeConfiguration : IEntityTypeConfiguration<PaymentMethodType>
{
    public void Configure(EntityTypeBuilder<PaymentMethodType> builder)
    {
        builder.ToTable("PaymentMethodType");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(30).IsRequired();
        builder.HasData(
            new PaymentMethodType { Id = 1, Name = "Card" },
            new PaymentMethodType { Id = 2, Name = "Transfer" },
            new PaymentMethodType { Id = 3, Name = "Cash" },
            new PaymentMethodType { Id = 4, Name = "PayPal" },
            new PaymentMethodType { Id = 5, Name = "Crypto" }
        );
    }
}

public class PaymentStatusTypeConfiguration : IEntityTypeConfiguration<PaymentStatusType>
{
    public void Configure(EntityTypeBuilder<PaymentStatusType> builder)
    {
        builder.ToTable("PaymentStatusType");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(20).IsRequired();
        builder.HasData(
            new PaymentStatusType { Id = 1, Name = "Pending" },
            new PaymentStatusType { Id = 2, Name = "Succeeded" },
            new PaymentStatusType { Id = 3, Name = "Failed" },
            new PaymentStatusType { Id = 4, Name = "Refunded" },
            new PaymentStatusType { Id = 5, Name = "Disputed" }
        );
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount).HasPrecision(12, 2);
        builder.Property(p => p.CurrencyCode).HasMaxLength(3).IsFixedLength().HasDefaultValue("USD");
        builder.Property(p => p.StatusId).HasDefaultValue((short)1);

        builder.Ignore(p => p.TransactionExternalId);
        builder.Ignore(p => p.GatewayResponse);
        builder.Ignore(p => p.PaidAt);
        builder.Ignore(p => p.RefundedAt);
        builder.Ignore(p => p.RefundReason);
        builder.Ignore(p => p.CreatedAt);
        builder.Ignore(p => p.UpdatedAt);

        builder.HasOne(p => p.PaymentMethod)
               .WithMany(pm => pm.Payments)
               .HasForeignKey(p => p.PaymentMethodId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Status)
               .WithMany(ps => ps.Payments)
               .HasForeignKey(p => p.StatusId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoice");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        
        builder.Property(i => i.CustomerName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.TaxId).HasMaxLength(20).IsRequired();
        builder.Property(i => i.Total).HasPrecision(12, 2);
        
        builder.Property(i => i.CreatedAt);

        builder.Ignore(i => i.UserId);
        builder.Ignore(i => i.Email);
        builder.Ignore(i => i.Address);
        builder.Ignore(i => i.Subtotal);
        builder.Ignore(i => i.TaxAmount);
        builder.Ignore(i => i.CurrencyCode);

        builder.HasMany(i => i.Details)
               .WithOne(d => d.Invoice)
               .HasForeignKey(d => d.InvoiceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
    {
        builder.ToTable("InvoiceDetail");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Description).HasMaxLength(255).IsRequired();
        builder.Property(d => d.UnitPrice).HasPrecision(12, 2);
        builder.Property(d => d.TotalItem).HasPrecision(12, 2);

        builder.Ignore(d => d.TaxRate);
    }
}
