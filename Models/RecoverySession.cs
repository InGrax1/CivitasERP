using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitasERP.Models
{
    class RecoverySession
    {
        public static int AdminId { get; set; }
        public static string Code { get; set; }
        public static DateTime Expires { get; set; }
    }
}
