using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime? DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null)
            {
                var message = $"Invalid Person `{request.Name}`";
                _context.LogError(message);
                throw new BadHttpRequestException($"Bad Request::{message}");
            }

            if (request.DutyStartDate is not null)
            {
                DateTime startDate = request.DutyStartDate?.Date ?? DateTime.Now.Date;

                var previousDuty = _context.AstronautDuties.OrderByDescending(z => z.DutyStartDate).FirstOrDefault(z => z.PersonId == person.Id);
                if (previousDuty is not null)
                {
                    if (previousDuty.DutyTitle == "RETIRED")
                    {
                        var message = $"Invalid Duty, Astronaut Retired`";
                        _context.LogError(message);
                        throw new BadHttpRequestException($"Bad Request::{message}");
                    }
                    if (previousDuty.DutyStartDate.Date >= startDate)
                    {
                        var message = $"Invalid Duty Start `{request.DutyStartDate}`";
                        _context.LogError(message);
                        throw new BadHttpRequestException($"Bad Request::{message}");
                    }
                }
            }

            var verifyNoPreviousDuty = _context.AstronautDuties.FirstOrDefault(z => z.DutyTitle == request.DutyTitle && z.DutyStartDate <= request.DutyStartDate);

            if (verifyNoPreviousDuty is not null)
            {
                var message = $"Duplicate Duty `{request.Name}`";
                _context.LogError(message);
                throw new BadHttpRequestException($"Bad Request::{message}");
            }

            _context.LogStatus($"CreateAstronautDutyPreProcessor valid");

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            DateTime dutyStartDate = request.DutyStartDate ?? DateTime.Now.AddDays(1);

            var query = @"SELECT * FROM [Person] WHERE Name=@Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<Person>(query, new
                {
                    Name = request.Name
                });

            if (person is null)
            {
                var message = $"Invalid Person: {request.Name}";
                _context.LogError(message);
                throw new BadHttpRequestException($"Bad Request::{message}");
            }

            query = @"SELECT * FROM [AstronautDetail] WHERE PersonId=@Id";

            var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(query, new
                {
                    Id = person.Id
                });
            if (astronautDetail is null)
            {
                astronautDetail = new AstronautDetail
                {
                    PersonId = person.Id,
                    CurrentDutyTitle = request.DutyTitle,
                    CurrentRank = request.Rank,
                    CareerStartDate = dutyStartDate.Date
                };
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = dutyStartDate.Date;
                }
                _context.LogStatus($"CREATE astronautDetail");
                await _context.AstronautDetails.AddAsync(astronautDetail);
            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = dutyStartDate.AddDays(-1).Date;
                }
                _context.LogStatus($"UPDATE astronautDetail");
                _context.AstronautDetails.Update(astronautDetail);
            }

            query = @"SELECT * FROM [AstronautDuty] WHERE PersonId=@Id Order By DutyStartDate Desc";

            var astronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query, new 
                {
                    Id = person.Id
                });

            if (astronautDuty is not null)
            {
                _context.LogStatus($"DUTY UPDATED ID: {person.Id} DUTY: {astronautDuty.DutyTitle}");
                astronautDuty.DutyEndDate = dutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = dutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            _context.LogStatus($"DUTY CREATED");

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
