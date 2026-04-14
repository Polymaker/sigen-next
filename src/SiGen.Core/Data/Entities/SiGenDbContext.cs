using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Data.Entities
{
    public class SiGenDbContext : DbContext
    {
        public virtual DbSet<StringSet> StringSets { get; set; } = null!;
        public virtual DbSet<StringSpec> StringSpecs { get; set; } = null!;

        public SiGenDbContext()
        {
        }

        public SiGenDbContext(DbContextOptions<SiGenDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=SiGenDatabase.db");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SiGenDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
