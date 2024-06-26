namespace StargateAPI.Business.Dtos
{
    public class LogItem
    {
        public string Id { get; set; }

        public DateTime EventDate { get; set; } = DateTime.Now;

        public string Type { get; set; } = "E";

        public required string Message { get; set; }
    }
}
