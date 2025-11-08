namespace SIMS.Core
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Loglevel { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
    }

}
