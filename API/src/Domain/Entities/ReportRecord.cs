using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class ReportRecord : BaseEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid? VendorId { get; private set; }          // null = admin report
        public Vendor? Vendor { get; private set; }
        public ReportScope Scope { get; private set; }
        public ReportFrequency Frequency { get; private set; }
        public string PdfStoragePath { get; private set; } = default!;
        public bool EmailSent { get; private set; }

        public static ReportRecord Create(
            Guid? vendorId,
            ReportScope scope,
            ReportFrequency frequency,
            string pdfPath) =>
            new()
            {
                VendorId = vendorId,
                Scope = scope,
                Frequency = frequency,
                PdfStoragePath = pdfPath,
                EmailSent = false
            };

        public void MarkEmailSent() => EmailSent = true;
    }
}
