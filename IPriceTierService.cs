using HKTDC.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ungerboeck.Api.Models.Subjects;

namespace HKTDC.SPCS.ResourceDeactivation.Interfaces
{
    public interface IPriceTierService
    {
        IEnumerable<PriceList> GetPriceListTier();
        void UpdatePriceListTier();
    }
}
