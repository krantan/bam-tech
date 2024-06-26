using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {

            var result = new GetAstronautDutiesByNameResult();

            var query = @"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE a.Name=@Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query, new
                {
                    Name = request.Name
                });

            if (person is null)
            {
                var message = $"Invalid Person `{request.Name}`";
                _context.LogError(message);
                throw new BadHttpRequestException($"Bad Request::{message}");
            }

            query = @"SELECT * FROM [AstronautDuty] WHERE PersonId=@Id Order By DutyStartDate Desc";

            var duties = await _context.Connection.QueryAsync<AstronautDuty>(query, new
                {
                    Id = person.PersonId
                });

            return new GetAstronautDutiesByNameResult()
            {
                Person = person,
                AstronautDuties = duties.ToList()
            };

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
