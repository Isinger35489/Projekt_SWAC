using System.ComponentModel.DataAnnotations;

namespace SIMS.Core.Classes
{
    public class Incident
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ReporterId { get; set; }

        [Required]
        public int HandlerId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Severity { get; set; }

        [Required]
        public string Status { get; set; }

        //optional daher kein required verwendet
        public string? CVE { get; set; }

        [Required]
        public int EscalationLevel { get; set; }

        [Required]
        public string System { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        //optional daher kein required verwendet
        public DateTime? ClosedAt { get; set; }
        }

    }

