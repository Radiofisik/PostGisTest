using System;
using System.Collections.Generic;
using System.Text;
using EFTest.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFTest
{
    public class BloggingContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }

        public DbSet<City> Cities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres",
                o => o.UseNetTopologySuite());
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("postgis");
        }
    }
}