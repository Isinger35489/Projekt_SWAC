namespace sims;

public class Incident
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string ReportedByID { get; set; }
    public int Escalation { get; set; }
    public DateTime CreatedAt { get; set; }
    


}