using Integrations.HKTDC.SPCS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrations.HKTDC.SPCS.ResourceImport.Model
{
    public class ResourceItem
    {
        public IEnumerable<Package>? Packages { get; set; }
        public IEnumerable<SellableItem>? SellableItems { get; set; }
    }
}
