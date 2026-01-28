using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.SKU)
            .IsUnique();

        builder.Property(p => p.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Model)
            .HasMaxLength(100);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.Specifications)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

