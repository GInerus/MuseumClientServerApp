using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseumClient.Models
{
    public class UserSession
    {
        public string status { get; set; }
        public string token { get; set; }
        public string userType { get; set; }
    }
}
