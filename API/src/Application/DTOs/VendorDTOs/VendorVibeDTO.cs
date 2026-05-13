using System.Collections.Generic;

namespace Application.DTOs.VendorDTOs
{
    public class VendorVibeDTO
    {
        public string VibeSummary { get; set; }
        public List<string> KeyStrengths { get; set; }
        public string OverallSentiment { get; set; }
    }
}
