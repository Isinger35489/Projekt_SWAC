namespace SIMS.Core
{
    public class Incident
    {
            public int Id { get; set; }
            public int ReporterId { get; set; }
            public int HandlerId { get; set; }
            public string Description { get; set; }
            public string Severity { get; set; }
            public string Status { get; set; }
            public string CVE { get; set; }
            public int EscalationLevel { get; set; }
            public string System { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ClosedAt { get; set; }
        }

    }

