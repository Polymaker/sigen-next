using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace SiGen.Data.Entities
{
    public class StringSet : IEntityTypeConfiguration<StringSet>
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty; // e.g., "EXL110 Regular Light"
        public string Brand { get; set; } = string.Empty; // e.g., "Ernie Ball"
        public string InstrumentType { get; set; } = "Guitar"; // Helpful for filtering

        public int NumberOfStrings { get; set; }

        // Navigation property for the join table
        public List<SetItem> Strings { get; set; } = new();

        public void Configure(EntityTypeBuilder<StringSet> builder)
        {
            builder.ToTable("StringSets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.Brand).IsRequired().HasMaxLength(255);
            builder.Property(x => x.InstrumentType).IsRequired().HasMaxLength(50);
            builder.HasMany(x => x.Strings)
                .WithOne(x => x.StringSet)
                .HasForeignKey(x => x.StringSetId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void SyncNumberOfStrings()
        {
            NumberOfStrings = Strings.Count;
        }
    }
}
