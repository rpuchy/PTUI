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
                        string alternate = "";
                        switch (name)
                        {
                            case "ProbabilityState1ToState2":
                                alternate = "probability_state1_state2";
                                break;
                            case "ProbabilityState2ToState1":
                                alternate = "probability_state2_state1";
                                break;
                            case "SweepCashFlows":
                                alternate = "sweep_cashflows";
                                break;
                            default:
                                {
                                    alternate = name.Replace("ID", "_id");
                                    alternate = Regex.Replace(alternate, @"(?<=.)([A-Z][a-z])", "_$1").ToLower();
                                    break;
                                }
                        }
                        
                        index = FindIndex(x => x.Name == alternate);
                    }
                    else
                    {
                        string alternate = "";
                        switch (name)
                        {
                            case "code": alternate=name;
                                         break;
                            case "probability_state1_state2": alternate = "ProbabilityState1ToState2";
                                                              break;
                            case "probability_state2_state1": alternate = "ProbabilityState2ToState1";
                                                              break;
                            case "sweep_cashflows": alternate = "SweepCashFlows";
                                                     break;
                            default:
                                {
                                    alternate = Regex.Replace(name, "_id", "ID");
                                    alternate = Regex.Replace(alternate, @"((_[a-z]))", m => m.ToString().ToUpper().Trim('_'));
                                    alternate = char.ToUpper(alternate[0]) + alternate.Substring(1);
                                    break;
                                }
                        }                        
                        index = FindIndex(x => x.Name == alternate);
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
                            string alternate = "";
                            switch (name)
                            {
                                case "code":
                                    alternate = name;
                                    break;
                                case "probability_state1_state2":
                                    alternate = "ProbabilityState1ToState2";
                                    break;
                                case "probability_state2_state1":
                                    alternate = "ProbabilityState2ToState1";
                                    break;
                                case "sweep_cashflows":
                                    alternate = "SweepCashFlows";
                                    break;
                                default:
                                {
                                    alternate = Regex.Replace(name, "_id", "ID");
                                    alternate = Regex.Replace(alternate, @"((_[a-z]))",
                                        m => m.ToString().ToUpper().Trim('_'));
                                    alternate = char.ToUpper(alternate[0]) + alternate.Substring(1);
                                    break;
                                }
                            }
                            index = FindIndex(x => x.Name == alternate);
                        }
                    }
                    this[index].Value = value;
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
