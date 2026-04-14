using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace SiGen.Data.Entities
{
    public class SetItem : IEntityTypeConfiguration<SetItem>
    {
        public int Id { get; set; }
        public int StringSetId { get; set; }
        public int StringSpecId { get; set; }

        public int SortOrder { get; set; }  // Position (1, 2, 3...)
        //public string DefaultNote { get; set; } = "E";

        // Navigation properties
        public StringSet StringSet { get; set; } = null!;
        public StringSpec String { get; set; } = null!;

        public void Configure(EntityTypeBuilder<SetItem> builder)
        {
            builder.ToTable("StringSetItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SortOrder).IsRequired();
            //builder.Property(x => x.DefaultNote).IsRequired().HasMaxLength(10);
            builder.HasOne(x => x.String)
                .WithMany()
                .HasForeignKey(x => x.StringSpecId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
