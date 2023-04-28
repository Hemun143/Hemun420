using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrations.HKTDC.SPCS.OrderImport.Model
{
    public class UserDefinedFieldModel
    {
        public string UserFieldName { get; set; }

        public dynamic userFieldValue { get; set; }
    }
}
