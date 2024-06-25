using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;


public class DbContextFactory : IDisposable
{
    private SqliteConnection _connection;

    private DbContextOptions<StargateContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<StargateContext>()
            .UseSqlite(_connection).Options;
    }

    public StargateContext CreateContext()
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = CreateOptions();
            using (var context = new StargateContext(options))
            {
                context.Database.EnsureCreated();
            }
        }

        return new StargateContext(CreateOptions());
    }

    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Dispose();
            _connection = null;
        }
    }
}