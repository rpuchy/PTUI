using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLib;

namespace PlanningTool
{
    class Product
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string TaxWrapper { get; set; }
        public IList<Parameter> Parameters { get; set; }

    }
}
