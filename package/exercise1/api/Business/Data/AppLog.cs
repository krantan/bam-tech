using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StargateAPI.Business.Data
{
    [Table("AppLog")]
    public class AppLog
    {
        public string Id { get; set; }

        public DateTime EventDate { get; set; } = DateTime.Now;

        public string Type { get; set; } = "E";

        public required string Message { get; set; }
    }

    public class AppLogConfiguration : IEntityTypeConfiguration<AppLog>
    {
        public void Configure(EntityTypeBuilder<AppLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
