using System;

namespace MuseumServer.Models
{
    public class Session
    {
        public int SessionId { get; set; } // PK
        public string Token { get; set; } = null!;
        public string UserType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccess { get; set; }
    }
}