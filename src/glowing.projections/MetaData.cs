using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glowing.projections
{
    public class MetaData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
