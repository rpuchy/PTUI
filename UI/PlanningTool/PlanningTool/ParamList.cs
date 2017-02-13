using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLib;

namespace PlanningTool
{
    class ParamList : List<Parameter>
    {
        public object this[string name]
        {
            get { return Find(x => x.Name == name).Value; }
            set { this[FindIndex(x => x.Name == name)].Value = value; }
        }
    }
}
