using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class ScheduledReport : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? VendorId { get; set; }
        public Vendor? Vendor { get; set; }
        public ReportScope Scope { get; set; }
        public ReportFrequency Frequency { get; set; }
        public string EmailRecipient { get; set; } = default!;
        public bool IsEnabled { get; set; } = true;
    }
}
