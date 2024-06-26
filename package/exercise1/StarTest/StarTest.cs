using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

namespace StarTest;

public class UnitTest1
{
    [Fact]
    public async Task Person_Create_Test()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var createHandler = new CreatePersonHandler(context);
                var createRequest = new CreatePerson()
                {
                    Name = "New Guy"
                };
                var createResult = await createHandler.Handle(createRequest, CancellationToken.None);
            }
        }
    }
    [Fact]
    public async Task Person_GetByName_Test()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var createHandler = new GetPersonByNameHandler(context);
                var createRequest = new GetPersonByName()
                {
                    Name = "John Doe"
                };
                var createResult = await createHandler.Handle(createRequest, CancellationToken.None);
                Assert.Equal("John Doe", createResult.Person?.Name);;
            }
        }
    }
    [Fact]
    public async Task Person_Unique_Throws()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var createHandler = new CreatePersonHandler(context);
                var createRequest = new CreatePerson()
                {
                    Name = "John Doe"
                };

                await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(() => createHandler.Handle(createRequest, CancellationToken.None));
            }
        }
    }

    [Fact]
    public async Task Astronaut_NoDuty_Test()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var getHandler = new GetPersonByNameHandler(context);
                var getRequest = new GetPersonByName(){
                    Name = "New Guy"
                };
                var getResult = await getHandler.Handle(getRequest, CancellationToken.None);
                Assert.Null(getResult.Person);
            }
        }
    }

    [Fact]
    public void Astronaut_SingleDuty_Test()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var command = factory.GetCommand(
                    @"select PersonId, count(*) AS cnt from [AstronautDuty] WHERE DutyEndDate is null group by PersonId HAVING cnt>1"
                );
                using (var reader = command.ExecuteReader())
                {
                    Assert.False(reader.HasRows);
                }
            }
        }
    }

    [Fact]
    public async Task Astronaut_DutyPeriod_Test()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var getHandler = new CreateAstronautDutyHandler(context);
                var getRequest = new CreateAstronautDuty(){
                    Name = "John Doe",
                    DutyTitle = "Spacewalk",
                    Rank = "2LT",
                    DutyStartDate = new DateTime(2024, 2, 1)
                };
                var getResult = await getHandler.Handle(getRequest, CancellationToken.None);
                Assert.NotNull(getResult.Id);
            }
        }
    }
    [Fact]
    public async Task Astronaut_DutyPeriod_Throws()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var getPreProcessor = new CreateAstronautDutyPreProcessor(context);
                var getRequest = new CreateAstronautDuty(){
                    Name = "Duty Test",
                    DutyTitle = "Spacewalk",
                    Rank = "2LT",
                    DutyStartDate = new DateTime(2023, 1, 1)
                };
                await Assert.ThrowsAsync<Microsoft.AspNetCore.Http.BadHttpRequestException>(async () => await getPreProcessor.Process(getRequest, CancellationToken.None));
            }
        }
    }
    [Fact]
    public async Task Astronaut_Retired_Throws()
    {
        using (var factory = new DbContextFactory())
        {
            using (var context = factory.CreateContext())
            {
                var getPreProcessor = new CreateAstronautDutyPreProcessor(context);
                var getRequest = new CreateAstronautDuty(){
                    Name = "Test Retired",
                    DutyTitle = "Spacewalk",
                    Rank = "2LT",
                    DutyStartDate = new DateTime(2023, 2, 1)
                };
                await Assert.ThrowsAsync<Microsoft.AspNetCore.Http.BadHttpRequestException>(async () => await getPreProcessor.Process(getRequest, CancellationToken.None));
            }
        }
    }
}
