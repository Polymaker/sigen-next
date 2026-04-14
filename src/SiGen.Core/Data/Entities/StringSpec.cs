using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiGen.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Data.Entities
{
    public class StringSpec : IEntityTypeConfiguration<StringSpec>
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty; // e.g., "NW022"
        public string Brand { get; set; } = string.Empty; // e.g., "D'Addario"
        public StringMaterialType MaterialType { get; set; }

        /// <summary>
        /// The gauge (diameter) of the string in inches.
        /// </summary>
        public double Gauge { get; set; }
        /// <summary>
        /// The diameter of the core wire (for wound strings) in inches.
        /// </summary>
        public double? CoreDiameter { get; set; }
        /// <summary>
        /// Gets or sets the unit weight in lbs/in³.
        /// </summary>
        public double? UnitWeight { get; set; }
        ///// <summary>
        ///// Gets or sets the Young's modulus in GPa.
        ///// </summary>
        //public double? YoungsModulus { get; set; }

        public void Configure(EntityTypeBuilder<StringSpec> builder)
        {
            builder.ToTable("StringSpecs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Brand).IsRequired().HasMaxLength(255);
            builder.Property(x => x.MaterialType)
                .HasConversion<string>()
                .IsRequired();
            builder.Property(x => x.Gauge).IsRequired();
            //builder.Property(x => x.UnitWeight).IsRequired();
            //builder.Property(x => x.YoungsModulus).IsRequired();

        }
    }
}
