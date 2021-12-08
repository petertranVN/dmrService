using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalrServer.Data
{
    public class UserJoinHub
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public int MachineID { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
