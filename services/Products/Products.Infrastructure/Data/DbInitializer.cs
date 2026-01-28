using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

        await context.Database.MigrateAsync();

        if (await context.Categories.AnyAsync())
            return;

        // Ana kategoriler
        var cpuCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "İşlemciler (CPU)",
            Description = "Masaüstü ve dizüstü bilgisayarlar için işlemciler",
            CreatedAt = DateTime.UtcNow
        };

        var gpuCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Ekran Kartları (GPU)",
            Description = "Oyun ve profesyonel kullanım için ekran kartları",
            CreatedAt = DateTime.UtcNow
        };

        var ramCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Bellekler (RAM)",
            Description = "DDR4 ve DDR5 RAM modülleri",
            CreatedAt = DateTime.UtcNow
        };

        var storageCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Depolama",
            Description = "SSD, HDD ve NVMe depolama çözümleri",
            CreatedAt = DateTime.UtcNow
        };

        var motherboardCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Anakartlar",
            Description = "Intel ve AMD platformları için anakartlar",
            CreatedAt = DateTime.UtcNow
        };

        var psuCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Güç Kaynakları (PSU)",
            Description = "Modüler ve yarı modüler güç kaynakları",
            CreatedAt = DateTime.UtcNow
        };

        var caseCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Kasalar",
            Description = "ATX, Micro-ATX ve Mini-ITX kasalar",
            CreatedAt = DateTime.UtcNow
        };

        var coolingCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Soğutma",
            Description = "Hava ve sıvı soğutma çözümleri",
            CreatedAt = DateTime.UtcNow
        };

        await context.Categories.AddRangeAsync(
            cpuCategory, gpuCategory, ramCategory, storageCategory,
            motherboardCategory, psuCategory, caseCategory, coolingCategory
        );

        // Örnek ürünler
        var products = new List<Product>
        {
            // CPU'lar
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Intel Core i9-14900K",
                Description = "24 çekirdekli (8P+16E), 32 thread, 5.8GHz boost saat hızı",
                SKU = "CPU-INT-I9-14900K",
                Brand = "Intel",
                Model = "i9-14900K",
                Price = 24999.99m,
                Stock = 15,
                CategoryId = cpuCategory.Id,
                Specifications = "{\"cores\": 24, \"threads\": 32, \"baseClock\": \"3.2GHz\", \"boostClock\": \"5.8GHz\", \"tdp\": \"125W\", \"socket\": \"LGA1700\"}",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "AMD Ryzen 9 7950X3D",
                Description = "16 çekirdekli, 32 thread, 3D V-Cache teknolojisi",
                SKU = "CPU-AMD-R9-7950X3D",
                Brand = "AMD",
                Model = "Ryzen 9 7950X3D",
                Price = 27999.99m,
                Stock = 10,
                CategoryId = cpuCategory.Id,
                Specifications = "{\"cores\": 16, \"threads\": 32, \"baseClock\": \"4.2GHz\", \"boostClock\": \"5.7GHz\", \"cache\": \"144MB\", \"socket\": \"AM5\"}",
                CreatedAt = DateTime.UtcNow
            },

            // GPU'lar
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "NVIDIA GeForce RTX 4090",
                Description = "24GB GDDR6X, Ada Lovelace mimarisi",
                SKU = "GPU-NV-RTX4090",
                Brand = "NVIDIA",
                Model = "RTX 4090",
                Price = 89999.99m,
                Stock = 5,
                CategoryId = gpuCategory.Id,
                Specifications = "{\"memory\": \"24GB GDDR6X\", \"cuda\": 16384, \"boostClock\": \"2.52GHz\", \"tdp\": \"450W\"}",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "AMD Radeon RX 7900 XTX",
                Description = "24GB GDDR6, RDNA 3 mimarisi",
                SKU = "GPU-AMD-RX7900XTX",
                Brand = "AMD",
                Model = "RX 7900 XTX",
                Price = 52999.99m,
                Stock = 8,
                CategoryId = gpuCategory.Id,
                Specifications = "{\"memory\": \"24GB GDDR6\", \"streamProcessors\": 6144, \"boostClock\": \"2.5GHz\", \"tdp\": \"355W\"}",
                CreatedAt = DateTime.UtcNow
            },

            // RAM
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "G.Skill Trident Z5 RGB DDR5",
                Description = "32GB (2x16GB) DDR5-6400 CL32",
                SKU = "RAM-GSKILL-TZ5-32GB",
                Brand = "G.Skill",
                Model = "Trident Z5 RGB",
                Price = 8999.99m,
                Stock = 25,
                CategoryId = ramCategory.Id,
                Specifications = "{\"capacity\": \"32GB (2x16GB)\", \"speed\": \"DDR5-6400\", \"latency\": \"CL32\", \"voltage\": \"1.4V\"}",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Corsair Dominator Platinum DDR5",
                Description = "64GB (2x32GB) DDR5-5600 CL36",
                SKU = "RAM-CORS-DOM-64GB",
                Brand = "Corsair",
                Model = "Dominator Platinum",
                Price = 14999.99m,
                Stock = 12,
                CategoryId = ramCategory.Id,
                Specifications = "{\"capacity\": \"64GB (2x32GB)\", \"speed\": \"DDR5-5600\", \"latency\": \"CL36\", \"voltage\": \"1.25V\"}",
                CreatedAt = DateTime.UtcNow
            },

            // Storage
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Samsung 990 Pro",
                Description = "2TB NVMe M.2 SSD, PCIe 4.0",
                SKU = "SSD-SAM-990PRO-2TB",
                Brand = "Samsung",
                Model = "990 Pro",
                Price = 6499.99m,
                Stock = 30,
                CategoryId = storageCategory.Id,
                Specifications = "{\"capacity\": \"2TB\", \"interface\": \"PCIe 4.0 x4\", \"readSpeed\": \"7450MB/s\", \"writeSpeed\": \"6900MB/s\"}",
                CreatedAt = DateTime.UtcNow
            },

            // Motherboard
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "ASUS ROG Maximus Z790 Hero",
                Description = "Intel Z790, DDR5, ATX anakart",
                SKU = "MB-ASUS-Z790HERO",
                Brand = "ASUS",
                Model = "ROG Maximus Z790 Hero",
                Price = 22999.99m,
                Stock = 7,
                CategoryId = motherboardCategory.Id,
                Specifications = "{\"socket\": \"LGA1700\", \"chipset\": \"Z790\", \"memory\": \"DDR5-7800+\", \"formFactor\": \"ATX\"}",
                CreatedAt = DateTime.UtcNow
            },

            // PSU
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Corsair HX1500i",
                Description = "1500W 80+ Platinum, tamamen modüler",
                SKU = "PSU-CORS-HX1500I",
                Brand = "Corsair",
                Model = "HX1500i",
                Price = 12999.99m,
                Stock = 10,
                CategoryId = psuCategory.Id,
                Specifications = "{\"wattage\": \"1500W\", \"efficiency\": \"80+ Platinum\", \"modular\": true, \"fanSize\": \"140mm\"}",
                CreatedAt = DateTime.UtcNow
            },

            // Case
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Lian Li O11 Dynamic EVO",
                Description = "Dual-chamber ATX kasa, temperli cam",
                SKU = "CASE-LIANLI-O11EVO",
                Brand = "Lian Li",
                Model = "O11 Dynamic EVO",
                Price = 5999.99m,
                Stock = 18,
                CategoryId = caseCategory.Id,
                Specifications = "{\"formFactor\": \"ATX/E-ATX\", \"material\": \"Aluminum/Tempered Glass\", \"fans\": \"10x 120mm\"}",
                CreatedAt = DateTime.UtcNow
            },

            // Cooling
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "NZXT Kraken Z73 RGB",
                Description = "360mm AIO sıvı soğutucu, LCD ekran",
                SKU = "COOL-NZXT-Z73RGB",
                Brand = "NZXT",
                Model = "Kraken Z73 RGB",
                Price = 9999.99m,
                Stock = 14,
                CategoryId = coolingCategory.Id,
                Specifications = "{\"radiator\": \"360mm\", \"fans\": \"3x 120mm\", \"display\": \"2.36 inch LCD\", \"tdpSupport\": \"350W+\"}",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Products database seeded successfully!");
    }
}

