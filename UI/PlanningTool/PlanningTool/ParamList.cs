using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLib;

namespace PlanningTool
{
    public class ParamList : List<Parameter>
    {
        public object this[string name]
        {
            get
            {
                try
                {
                    return Find(x => x.Name == name).Value;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            set
            {
                //TODO put logic in here to try alternative. 

                this[FindIndex(x => x.Name == name)].Value = value;
            }
        }
    }
}
