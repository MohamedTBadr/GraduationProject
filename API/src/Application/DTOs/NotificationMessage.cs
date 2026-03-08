using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class NotificationMessage
    {
        public string RecipientId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

}
