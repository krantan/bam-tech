using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetLogs : IRequest<GetLogsResult>
    {

    }

    public class GetLogsHandler : IRequestHandler<GetLogs, GetLogsResult>
    {
        public readonly StargateContext _context;
        public GetLogsHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<GetLogsResult> Handle(GetLogs request, CancellationToken cancellationToken)
        {
            var query = @"SELECT * FROM AppLog ORDER BY EventDate DESC LIMIT 20";

            var logs = await _context.Connection.QueryAsync<LogItem>(query);

            _context.LogStatus($"QUERY: GetLogsHandler COUNT: {logs.Count()}");

            return new GetLogsResult()
            {
                Logs = logs.ToList()
            };
        }
    }

    public class GetLogsResult : BaseResponse
    {
        public List<LogItem> Logs { get; set; } = new List<LogItem> { };

    }
}
