using System;
using System.ComponentModel.DataAnnotations;

namespace MuseumServer.Data
{
    public class Session
    {
        [Key]
        public string Token { get; set; } = Guid.NewGuid().ToString();
        public string UserType { get; set; } = "guest";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime LastAccess { get; set; } = DateTime.Now;
    }
}