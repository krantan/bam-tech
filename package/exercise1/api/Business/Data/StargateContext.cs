using Microsoft.EntityFrameworkCore;
using System.Data;

namespace StargateAPI.Business.Data
{
    public class StargateContext : DbContext
    {
        public IDbConnection Connection => Database.GetDbConnection();
        public DbSet<AppLog> Log { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<AstronautDetail> AstronautDetails { get; set; }
        public DbSet<AstronautDuty> AstronautDuties { get; set; }

        public StargateContext(DbContextOptions<StargateContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StargateContext).Assembly);

            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public async void LogStatus(string message)
        {
            await LogItem(new AppLog()
            {
                Id = System.Guid.NewGuid(),
                Message = message,
                Type = "S"
            });
        }

        public async void LogError(string message)
        {
            await LogItem(new AppLog()
            {
                Id = System.Guid.NewGuid(),
                Message = message,
                Type = "E"
            });
        }

        private async Task LogItem(AppLog log)
        {
            await this.Log.AddAsync(log);

            await this.SaveChangesAsync();
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            //add seed data
            modelBuilder.Entity<Person>()
                .HasData(
                    new Person
                    {
                        Id = 1,
                        Name = "John Doe"
                    },
                    new Person
                    {
                        Id = 2,
                        Name = "Jane Doe"
                    },
                    new Person
                    {
                        Id = 3,
                        Name = "Duty Test"
                    },
                    new Person
                    {
                        Id = 4,
                        Name = "Test Retired"
                    }
                );

            modelBuilder.Entity<AstronautDetail>()
                .HasData(
                    new AstronautDetail
                    {
                        Id = 1,
                        PersonId = 1,
                        CurrentRank = "1LT",
                        CurrentDutyTitle = "Commander",
                        CareerStartDate = new DateTime(2024, 1, 1)
                    },
                    new AstronautDetail
                    {
                        Id = 2,
                        PersonId = 3,
                        CurrentRank = "1LT",
                        CurrentDutyTitle = "Space Cowboy",
                        CareerStartDate = new DateTime(2024, 1, 1)
                    },
                    new AstronautDetail
                    {
                        Id = 3,
                        PersonId = 4,
                        CurrentRank = "1LT",
                        CurrentDutyTitle = "RETIRED",
                        CareerStartDate = new DateTime(2024, 1, 1)
                    }
                );

            modelBuilder.Entity<AstronautDuty>()
                .HasData(
                    new AstronautDuty
                    {
                        Id = 1,
                        PersonId = 1,
                        DutyStartDate = new DateTime(2024, 1, 1),
                        DutyTitle = "Commander",
                        Rank = "1LT"
                    },
                    new AstronautDuty
                    {
                        Id = 2,
                        PersonId = 3,
                        DutyStartDate = new DateTime(2024, 1, 1),
                        DutyTitle = "Space Cowboy",
                        Rank = "1LT"
                    },
                    new AstronautDuty
                    {
                        Id = 3,
                        PersonId = 3,
                        DutyStartDate = new DateTime(2024, 1, 1),
                        DutyTitle = "Space Cowboy",
                        Rank = "1LT"
                    },
                    new AstronautDuty
                    {
                        Id = 4,
                        PersonId = 4,
                        DutyStartDate = new DateTime(2022, 1, 1),
                        DutyTitle = "Navigation",
                        Rank = "1LT"
                    },
                    new AstronautDuty
                    {
                        Id = 5,
                        PersonId = 4,
                        DutyStartDate = new DateTime(2023, 1, 1),
                        DutyTitle = "RETIRED",
                        Rank = "1LT"
                    }
                );
        }
    }
}
