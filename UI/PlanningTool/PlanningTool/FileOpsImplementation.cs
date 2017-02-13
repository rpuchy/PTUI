﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.PerformanceData;
using System.Windows;
using System.Windows.Documents;
using System.Xml;
using BusinessLib;
using PlanningTool;
using TreeViewWithViewModelDemo.TextSearch;
using System.Linq;
using System.Xml.Linq;


namespace RequestRepresentation
{
    class FileOpsImplementation : BaseFileOps
    {   //
        private EngineObject _engineObjectTree = new EngineObject();
        private Dictionary<string, string[]> OutputTypeMap = new Dictionary<string, string[]>();
        private string _filename;

        public XmlDocument xmlDoc;
        
        
        public FileOpsImplementation(string filename)
        {
            _filename = filename;
            BuildValuetypeDictionary();
            LoadFile(_filename);            
        }        


        private void BuildValuetypeDictionary()
        {
            OutputTypeMap.Clear();
            using (var fs = File.OpenRead(@".\Valuetypes.csv"))
            using (var reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    //remove whitespace from items.
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = values[i].Trim();
                    }                    
                    OutputTypeMap.Add(values[0], values.Skip(1).ToArray());
                }
            }
        }


        public EngineObject EngineObjectTree
        {
            get { return _engineObjectTree; }
            set { _engineObjectTree = value; }
        }

        public string FileName { get; set; }

        public override void ProcessFile(string path)
        {
            throw new NotImplementedException();
        }


        protected void LoadFile( string path )
        {
            
            try {
                using ( FileStream file_reader = new FileStream( path, FileMode.Open ) )
                using ( XmlReader reader = XmlReader.Create( file_reader ) )
                {
                    XmlDocument _xmlDoc = new XmlDocument();
                    _xmlDoc.Load( reader );
                    buildTree(_xmlDoc);
                }
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( "ERROR: " + ex.Message + ex.ToString() +
                                                    Environment.NewLine );
                throw;
            }
        }

        public bool Save()
        {
            return SaveAs(_filename);
        }

        public bool SaveAs(string path)
        {
            _filename = path; //update the file that we point to.
            XmlDocument _xmldoc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = _xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = _xmldoc.DocumentElement;
            _xmldoc.InsertBefore(xmlDeclaration, root);

            _xmldoc.AppendChild(processModel(_xmldoc, EngineObjectTree));

            _xmldoc.Save(path);
            return true;
        }

        private XmlElement processModel(XmlDocument doc, EngineObject node)
        {
            XmlElement element = doc.CreateElement(string.Empty, node.NodeName, string.Empty);
            foreach (var param in node.Parameters)
            {
                var newparam = doc.CreateElement(string.Empty, param.Name, string.Empty);
                newparam.AppendChild(doc.CreateTextNode(param.Value));
                element.AppendChild(newparam);
            }
            foreach (var child in node.Children)
            {
                var newelement = processModel(doc, child);
                element.AppendChild(newelement);
            }
            return element;
        }

        


        public bool UpdateModel(EngineObjectViewModel rootnode)
        {
            //This will update the _engineobjecttree for any changes the user has made in the UI
            EngineObjectTree = new EngineObject(rootnode);

            return (!EngineObjectTree.isNull());
        }
        

        public bool Save(string path, EngineObjectViewModel rootnode)
        {
            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            doc.AppendChild(processModel(doc, rootnode));
            
            doc.Save(path);
            return true;

        }

        private XmlElement processModel(XmlDocument doc, EngineObjectViewModel node)
        {
            XmlElement element = doc.CreateElement(string.Empty, node.NodeName, string.Empty);
            foreach (var param in node.Parameters)
            {
                var newparam = doc.CreateElement(string.Empty, param.Name, string.Empty);
                newparam.AppendChild(doc.CreateTextNode(param.Value));
                element.AppendChild(newparam);
            }
            foreach (var child in node.Children)
            {
                var newelement = processModel(doc, child);
                element.AppendChild(newelement);
            }            
            return element;
        }

        private void buildTree(XmlDocument xmlDoc)
        {
            _engineObjectTree.Name = xmlDoc.LastChild.Name;
            _engineObjectTree.NodeName = xmlDoc.LastChild.Name;
            _engineObjectTree.Children = processChildren(xmlDoc.LastChild); //we use the first child because the main node is the document header
            _engineObjectTree.Parameters = processParameters(xmlDoc.LastChild);

        }

        //checks if the _node has any non parameter children
        private bool hasChildren(XmlNode _node)
        {
            int count = 0;
            foreach (XmlNode child in _node.ChildNodes)
            {
                if (child.Name != "#text" && child.Name != "#comment")
                {
                    count++;
                }
            }
            return (count > 0);
        }

        private IList<Parameter> processParameters(XmlNode _node)
        {
            List<Parameter> temp =new List<Parameter>();
            if (_node.ChildNodes.Count > 0)
            {
                foreach (XmlNode child in _node.ChildNodes)
                {
                    if (isParameter(child))
                    {
                        //if there are no children then it's a parameter
                        string val = child.FirstChild.Value;
                        string pname = child.Name;
                        temp.Add(new Parameter() {Name = pname, Value = val});
                    }
                }
            }
            return temp;
        }

        private string getName(XmlNode _node)
        {            
            foreach (XmlNode child in _node.ChildNodes)
            {
                if (child.Name == "Name" )
                {
                    return child.InnerText;
                }
            }
            //If we haven't found the name yet then search the Params sub folder for it.
            foreach (XmlNode child in _node.ChildNodes)
            {
                if (child.Name == "Params")
                {
                    foreach (XmlNode l2child in child.ChildNodes)
                    {
                        if (l2child.Name == "Name")
                        {
                            return l2child.InnerText;
                        }
                    }
                }
            }


            return _node.Name;
        }

        private bool isParameter(XmlNode _node)
        {
            return (_node.ChildNodes.Count == 1 && _node.FirstChild.Name == "#text");
        }

        private bool isComment(XmlNode _node)
        {
            return (_node.Name == "#comment");
        }


        private List<EngineObject> processChildren(XmlNode _node)
        {
            List<EngineObject> temp = new List<EngineObject>();
            if (!hasChildren(_node))
            {
                return temp;
            }
            foreach (XmlNode child in _node.ChildNodes)
            {
                //First Check if the child has more children or only text
                //1. children is only 1 
                //2. child is #text 
                if (!isParameter(child)&&!isComment(child))
                {
                    temp.Add(new EngineObject
                    {
                        Name = getName(child),
                        NodeName = child.Name,
                        Children = processChildren(child),
                        Parameters = processParameters(child)
                    });
                }
            }
            return temp;
        }

        //returns the ouput component
        public void AddAlloutputs(int timestepstart, int timestepend)
        {
            //We follow the following process
            //1. we cycle through the ESG models and pick out all the models 
            //2. we cycle through the products and pick out the products
            //first remove all the products
            Removealloutputs();

            //Add the transactions log, with scenario 1

            AddtransactionLog("c:\\Foresight\\results\\transactionlog.csv",new int[] {1});

            var Params = FindObjectNodeName("Params", EngineObjectTree);

            Params.Parameters = SetParam("Scenarios", Params.Parameters, 1.ToString());

            Params.Parameters = SetParam("InflationAdjusted", Params.Parameters, "false");

            Params.Parameters = SetParam("output_file", Params.Parameters, "c:\\Foresight\\results\\results.csv");

            //convert ESG to deterministic
            ConvertToDeterministic();
            
            var productlist = GetProducts(EngineObjectTree, "");
            var modeList = GetModels(EngineObjectTree);
            foreach (var prod in productlist)
            {
                string[] Valuetypes = OutputTypeMap[prod.Type];
                foreach (string vtype in Valuetypes)
                {
                    AddProductOutput(prod.Name, vtype, prod.TaxWrapper, timestepstart.ToString(), timestepend.ToString());
                }               
            }
            foreach (var model in modeList)
            {
                string[] Valuetypes = OutputTypeMap[model.Type];
                foreach (string vtype in Valuetypes)
                {
                    AddModelOutput(model.ID ,model.Name, vtype, timestepstart.ToString(), timestepend.ToString());
                }
            }
        }


        private void ConvertToDeterministic()
        {
            var hw = FindObjectNodeName("ModelParameters", FindObjectName("AUYieldCurve", EngineObjectTree));
            hw.Parameters = SetParam("VolatilityShortRate", hw.Parameters, "0.000000000001");
            hw.Parameters = SetParam("VolatilityAdditionalParam", hw.Parameters, "0.000000000001");


            var cpi = FindObjectName("CPI", EngineObjectTree);
            cpi.Parameters = SetParam("sigma", cpi.Parameters, "0.0000000000001", "Sigma");
            //TODO: we should set the mpr to mpr*sigmaold/sigmanew
            var awe = FindObjectName("AWE", EngineObjectTree);
            awe.Parameters = SetParam("sigma", awe.Parameters, "0.0000000000001", "Sigma");

            //Create one const vol model and set all other assumptions to correct mean return in model.
            //Find all models of type equity
            var modeList = GetModelsEO(EngineObjectTree);
            List<EngineObject> replacementEngineObjects = new List<EngineObject>();
            foreach (EngineObject model in modeList)
            {

                var mtype = GetParam("Type", model.Parameters);
                if (mtype == "EQUITY")
                {
                    var volID = GetParam("VolatilityModelID", model.Parameters, "volatility_model_id");
                    var volModel = modeList.Find(x => GetParam("ModelID", x.Parameters, "model_id") == volID);
                    double expectedReturn = 0.0;
                    if (GetParam("Type", volModel.Parameters) == "REGIMESWITCHVOLATILITY")
                    {
                        var p12 =Double.Parse(GetParam("probability_state1_state2", volModel.Parameters, "ProbabilityState1Stat2"));
                        var p21 = Double.Parse(GetParam("probability_state2_state1", volModel.Parameters, "ProbabilityState2Stat1"));
                        var mu1 = Double.Parse(GetParam("mean_return_state1", volModel.Parameters, "MeanReturnState1"));
                        var mu2 = Double.Parse(GetParam("mean_return_state2", volModel.Parameters, "MeanReturnState2"));
                        var sigma1 = Double.Parse(GetParam("volatility_state1", volModel.Parameters, "VolatilityState1"));
                        var sigma2 = Double.Parse(GetParam("volatility_state2", volModel.Parameters, "VolatilityState2"));

                        var prob2 = (p12) / (p12 + p21);
                        var prob1 = 1 - prob2;

                        expectedReturn = prob1 * (mu1 - 0.5 * sigma1 * sigma1) + prob2 * (mu2 - 0.5 * sigma2 * sigma2);
                    }
                    else
                    {
                        expectedReturn = Double.Parse(GetParam("mean_return", volModel.Parameters, "MeanReturn")) - 0.5*Math.Pow(Double.Parse(GetParam("volatility", volModel.Parameters, "Volatility")),2);
                    }
                    expectedReturn = Math.Round(expectedReturn, 8);
                    var tempmodel = new EngineObject()
                    {
                        Name = model.Name,
                        NodeName = model.NodeName,
                        Children = new List<EngineObject>(),
                        Parameters = new List<Parameter>()
                    };
                    tempmodel.Parameters.Add(new Parameter() {Name="Type",Value="DETERMINISTICEQUITY"});
                    tempmodel.Parameters.Add(new Parameter() { Name = "ModelID", Value = GetParam("ModelID", model.Parameters, "model_id")});
                    tempmodel.Parameters.Add(new Parameter() { Name = "Name", Value = GetParam("Name", model.Parameters, "Name") });
                    tempmodel.Parameters.Add(new Parameter() { Name = "UseNominalRates", Value = GetParam("use_nominal_rates", model.Parameters, "UseNominalRates") });
                    tempmodel.Parameters.Add(new Parameter() { Name = "NominalRatesModelID", Value = GetParam("nominal_rates_model_id", model.Parameters, "NominalRatesModelID") });
                    tempmodel.Parameters.Add(new Parameter() { Name = "MeanReturn", Value = expectedReturn.ToString()});
                    tempmodel.Parameters.Add(new Parameter() { Name = "IncomeModelID", Value = "0" });
                    replacementEngineObjects.Add(tempmodel);                                        
                }
            }

            var Models = FindObjectNodeName("Models", EngineObjectTree);

            List<EngineObject> removeList = new List<EngineObject>();

            foreach (EngineObject child in Models.Children)
            {
                var mtype = GetParam("Type", child.Parameters);
                if (mtype == "EQUITY"|| mtype == "REGIMESWITCHVOLATILITY" ||mtype== "CONSTANTVOLATILITY")
                {
                    removeList.Add(child);
                }
            }

            foreach (var engineObject in removeList)
            {
                Models.Children.Remove(engineObject);
            }

            foreach (EngineObject replacementEngineObject in replacementEngineObjects)
            {
                Models.Children.Add(replacementEngineObject);
            }

            var Corr = FindObjectNodeName("Correlations", EngineObjectTree);
            Corr.Children.Clear();
        }



        public void Removealloutputs()
        {
            foreach (var child in EngineObjectTree.Children)
            {
                if (child.Name == "OutputRequirements")
                {
                    foreach (var child2 in child.Children)
                    {
                        if (child2.Name == "Queries")
                        {
                            child2.Children.Clear();
                        }
                        if (child2.Name == "Operators")
                        {
                            child2.Children.Clear();
                        }
                    }
                }
            }
        }

        public void AddtransactionLog(string outputfilename, int[] scenarios)
        {
            EngineObject tlog_object = new EngineObject() { Name = "TransactionLog", NodeName = "TransactionLog", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            tlog_object.Parameters.Add(new Parameter() { Name = "LogFile", Value = outputfilename });
            foreach (int scenario in scenarios)
            {
                EngineObject scenarios_object = new EngineObject() { Name = "Scenarios", NodeName = "Scenarios", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
                scenarios_object.Parameters.Add(new Parameter() { Name = "Scenarios", Value = scenario.ToString() });
                tlog_object.Children.Add(scenarios_object);
            }
            var Params = FindObjectNodeName("Params", EngineObjectTree);
            Params.Children.Add(tlog_object);
        }


        private List<Model> GetModels(EngineObject node)
        {
            var temp = new List<Model>();            
            foreach (var child in node.Children)
            {
                temp.AddRange(GetModels(child));
            }
            if (node.NodeName == "Model")
            {
                temp.Add(new Model() {Name  = GetParam("Name", node.Parameters), ID = GetParam("ModelID",node.Parameters), Type = GetParam("Class", node.Parameters,"Type") });
            }
            return temp;
        }

        private List<EngineObject> GetModelsEO(EngineObject node)
        {
            var temp = new List<EngineObject>();
            foreach (var child in node.Children)
            {
                temp.AddRange(GetModelsEO(child));
            }
            if (node.NodeName == "Model")
            {
                temp.Add(node);
            }
            return temp;
        }


        private string GetParam(string ParamName, IList<Parameter> Parameters, string alternate= "")
        {
            var param = ((List<Parameter>) Parameters).Find(x => x.Name == ParamName);
            if (param == null&&alternate!="")
            {
                param = ((List<Parameter>)Parameters).Find(x => x.Name == alternate);
            }


            return (param == null)?  string.Empty : param.Value ;
        }

        private IList<Parameter> SetParam(string ParamName, IList<Parameter> Parameters, string value, string alternate = "")
        {
            var param = ((List<Parameter>)Parameters).Find(x => x.Name == ParamName);
            if (param == null && alternate != "")
            {
                param = ((List<Parameter>)Parameters).Find(x => x.Name == alternate);
            }
            //Parameters[((List<Parameter>) Parameters).FindIndex(x => x.Name == ParamName)].Value = value;
            param.Value = value;
            return Parameters;
        }


        private List<Product> GetProducts(EngineObject node, string prev_taxwrapper)
        {
            var temp = new List<Product>();
            string taxWrapper = "";
            if (node.NodeName == "TaxWrapper")
            {
                taxWrapper = node.Name;
            }
            else
            {
                taxWrapper = prev_taxwrapper;
            }
            foreach (var child in node.Children)
            {                
                temp.AddRange(GetProducts(child,taxWrapper));
            }
            if (node.NodeName == "Product")
            {
                temp.Add(new Product() {TaxWrapper = taxWrapper, Name= node.Name, Type=getProductType(node )});
            }
            return temp;
        }

        private string getProductType(EngineObject _node)
        {
            foreach (var param in _node.Parameters)
            {
                if (param.Name == "Type")
                {
                    return param.Value;
                }

            }
            //if we haven;t found it search the params tag            
            foreach (EngineObject child in _node.Children)
            {
                if (child.NodeName == "Params")
                {
                    foreach (Parameter param in child.Parameters)
                    {
                        if (param.Name == "Type")
                        {
                            return param.Value;
                        }
                    }
                }
            }
            return null;
        }


        private EngineObject newQueryFilter(string Field, string Value, string ObjectName)
        {

            EngineObject QueryFilterCriter1 = new EngineObject() { Name = "QueryFilterCriteria", NodeName = "QueryFilterCriteria", Parameters = new List<Parameter>() };
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Field", Value = Field });
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Value", Value = Value });
            EngineObject Where = new EngineObject() { Name = "Where", NodeName = "Where", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Where.Children.Add(QueryFilterCriter1);
            EngineObject QueryFilter = new EngineObject() { Name = "QueryFilter", NodeName = "QueryFilter", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            QueryFilter.Children.Add(Where);
            QueryFilter.Parameters.Add(new Parameter() { Name = "ObjectName", Value = ObjectName });

            return QueryFilter;
        }



        private EngineObject Value(string Valuetype)
        {

            EngineObject ValueTypes = new EngineObject() { Name = "ValueTypes", NodeName = "ValueTypes", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            ValueTypes.Parameters.Add(new Parameter() { Name = "ValueType", Value = Valuetype });

            EngineObject Values = new EngineObject() { Name = "Values", NodeName = "Values", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Values.Children.Add(ValueTypes);

            return Values;
        }

        private EngineObject newProductQuery(string QueryID, string productName, string taxWrapper, string ValueType)
        {
            EngineObject QueryFilter1 = newQueryFilter("Name", productName, "product");
            EngineObject QueryFilter2 = newQueryFilter("Name", taxWrapper, "tax_wrapper");

            EngineObject Filter = new EngineObject() { Name = "Filter", NodeName = "Filter", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Filter.Children.Add(QueryFilter1);
            Filter.Children.Add(QueryFilter2);

            EngineObject Query = new EngineObject() { Name = "Query", NodeName = "Query", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Query.Parameters.Add(new Parameter() { Name = "QueryID", Value = QueryID });
            Query.Parameters.Add(new Parameter() { Name = "Type", Value = "SIMVALUE" });
            Query.Children.Add(Value(ValueType));

            Query.Children.Add(Filter);
            return Query;
        }


        private EngineObject newModelQuery(string QueryID, string modelID, string ValueType)
        {
            EngineObject QueryFilter1 = newQueryFilter("ModelID", modelID, "model");

            EngineObject Filter = new EngineObject() { Name = "Filter", NodeName = "Filter", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Filter.Children.Add(QueryFilter1);


            EngineObject Query = new EngineObject() { Name = "Query", NodeName = "Query", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Query.Parameters.Add(new Parameter() { Name = "QueryID", Value = QueryID });
            Query.Parameters.Add(new Parameter() { Name = "Type", Value = "SIMVALUE" });
            Query.Children.Add(Value(ValueType));

            Query.Children.Add(Filter);
            return Query;
        }


        private EngineObject newOperator(string operatorID, string queryID, string valuetype, string timestepstart, string timestepend)
        {
            EngineObject OperationApplyTo = new EngineObject() {Name= "OperationApplyTo", NodeName = "OperationApplyTo", Children = new List<EngineObject>(), Parameters = new List<Parameter>()};
            OperationApplyTo.Parameters.Add(new Parameter() {Name= "QueryID", Value=queryID});
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "Value", Value = valuetype });
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "TimeStepStart", Value = timestepstart });
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "TimeStepEnd", Value = timestepend });

            EngineObject ApplyTo = new EngineObject() {Name="ApplyTo",NodeName = "ApplyTo", Children = new List<EngineObject>(), Parameters = new List<Parameter>()};
            ApplyTo.Children.Add(OperationApplyTo);

            EngineObject Operation = new EngineObject() { Name = "Operation", NodeName = "Operation", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Operation.Children.Add(ApplyTo);
            Operation.Parameters.Add(new Parameter() {Name="Name",Value= "Scenarios" });
            Operation.Parameters.Add(new Parameter() { Name = "Type", Value = "SCENARIOALL" });

            EngineObject Operator = new EngineObject() { Name = "Operator", NodeName = "Operator", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Operator.Children.Add(Operation);
            Operator.Parameters.Add(new Parameter() {Name= "OperatorID", Value=operatorID});

            return Operator;
        }

        private void AddProductOutput(string productName, string valueType, string taxWrapper, string timestepstart, string timestepend)
        {

            EngineObject QueryPart = newProductQuery("Query_" + taxWrapper + "_"+ productName + "_" + valueType, productName, taxWrapper,valueType);
            EngineObject OperatorPart = newOperator(taxWrapper+"_"+productName + "_" + valueType,
                "Query_" + taxWrapper + "_" + productName + "_" + valueType, valueType, timestepstart, timestepend);


            var queries = FindObjectNodeName("Queries", EngineObjectTree);
            var operators = FindObjectNodeName("Operators", EngineObjectTree);

            queries.Children.Add(QueryPart);
            operators.Children.Add(OperatorPart);

      
        }



        private void AddModelOutput(string modelID, string Name, string valueType, string timestepstart, string timestepend)
        {

            EngineObject QueryPart = newModelQuery("Query_" + modelID + "_" + valueType, modelID, valueType);
            EngineObject OperatorPart = newOperator("Oper_" +modelID +"_"+ Name + "_" + valueType,
                "Query_" + modelID + "_" + valueType, valueType, timestepstart, timestepend);

            var queries = FindObjectNodeName("Queries", EngineObjectTree);
            var operators = FindObjectNodeName("Operators", EngineObjectTree);

            queries.Children.Add(QueryPart);
            operators.Children.Add(OperatorPart);

        }


        
        private static EngineObject FindObjectNodeName(string name, EngineObject startingNode)
        {
            foreach (var child in startingNode.Children)
            {
                if (child.NodeName == name)
                {
                    return child;
                }
                var e_obj = FindObjectNodeName(name, child);
                if (e_obj != null)
                {
                    return e_obj;
                }
            }
            return null;
        }

        private static EngineObject FindObjectName(string name, EngineObject startingNode)
        {
            foreach (var child in startingNode.Children)
            {
                if (child.Name == name)
                {
                    return child;
                }
                var e_obj = FindObjectName(name, child);
                if (e_obj != null)
                {
                    return e_obj;
                }
            }
            return null;
        }

        private EngineObject economicModel(List<EngineObject> models, string modelName, string currency, string calibration_type)
        {
            var tempo = new EngineObject() {Name="economic_model",NodeName = "economic_model", Children = new List<EngineObject>(), Parameters = new List<Parameter>()};
            tempo.Parameters.Add(new Parameter() {Name = "model_name", Value=modelName});
            tempo.Parameters.Add(new Parameter() { Name = "currency", Value = currency});
            tempo.Parameters.Add(new Parameter() { Name = "current_mean_return", Value = "0"});
            var calType = new EngineObject() {Name="types", NodeName = "types", Parameters = new List<Parameter>(), Children = new List<EngineObject>()};
            calType.Parameters.Add(new Parameter() {Name = "calibration_type",Value=calibration_type});
            tempo.Children.Add(calType);
            foreach (var engineObject in models)
            {
                tempo.Children.Add(engineObject);
            }
            return tempo;
        }

        private void CalibrationsTemplate(string outputfilename)
        {
            

            string EffectiveDate = GetParam("EffectiveDate", FindObjectNodeName("Params", EngineObjectTree).Parameters,
                "effective_date");

            var Models = FindObjectNodeName("Models", EngineObjectTree);

            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            


            doc.AppendChild(processModel(doc, rootnode));

            doc.Save(outputfilename);


        }

        //
        /// <summary>
        /// Read and return a DataSet from a given XML file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>
        /// DataSet
        /// </returns>
        public DataSet LoadDatasetFromXml( string fileName )
        {
            DataSet ds = new DataSet();
            FileStream fs = null;
            try {
                fs = new FileStream( fileName, FileMode.Open, FileAccess.Read );
                using ( StreamReader reader = new StreamReader( fs ) )
                {
                    ds.ReadXml( reader );
                }
            }
            catch ( Exception e ) {
                System.Diagnostics.Debug.WriteLine( e.Data );
            }
            finally {
                if ( fs != null )
                    fs.Close();
            }
            return ds;
        }
    } // end FileOpsImplementation

}// end HelloObjectListView
