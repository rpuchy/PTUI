using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                int index = FindIndex(x => x.Name == name);
                if (index == -1)
                {
                    if (name.Any(c => char.IsUpper(c)))
                    {
                        string alternate = name.Replace("ID", "_id");
                        alternate = Regex.Replace(alternate, @"(?<=.)([A-Z][a-z])", "_$1").ToLower();
                        index = FindIndex(x => x.Name == alternate);
                    }
                    else
                    {
                        string alternate = Regex.Replace(name, @"(?<!_)([A-Z])", "\\U$1");
                        alternate = char.ToUpper(alternate[0]) + alternate.Substring(1);
                        index = FindIndex(x => x.Name == alternate.Trim('_'));
                    }                    
                }
                if (index >= 0)
                {
                    return this[index].Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    this[FindIndex(x => x.Name == name)].Value = value;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }                
            }
        }
    }
}
