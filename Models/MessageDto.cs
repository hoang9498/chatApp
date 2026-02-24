using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatApp.Models
{
    public class MessageDto
    {
        public required string chatroomId { get; set; }
        public required string message { get; set; }
        public required string type { get; set; }
    }
}