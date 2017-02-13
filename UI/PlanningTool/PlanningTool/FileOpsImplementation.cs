using System;
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
                newparam.AppendChild(doc.CreateTextNode(param.Value.ToString()));
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
                newparam.AppendChild(doc.CreateTextNode(param.Value.ToString()));
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

        private ParamList processParameters(XmlNode _node)
        {
            ParamList temp =new ParamList();
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

            Params.Parameters["Scenarios"] = 1.ToString(); //SetParam("Scenarios", Params.Parameters, 1.ToString());

            Params.Parameters["InflationAdjusted"] = "false";// SetParam("InflationAdjusted", Params.Parameters, "false");

            Params.Parameters["output_file"] = "c:\\Foresight\\results\\results.csv"; //SetParam("output_file", Params.Parameters, );

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
            var hw = EngineObjectTree.FindObject("AUYieldCurve","Name").FindObject("ModelParameters");// FindObjectNodeName("ModelParameters", FindObjectName("AUYieldCurve", EngineObjectTree));
            hw.Parameters["VolatilityShortRate"] = "0.000000000001"; //SetParam("VolatilityShortRate", hw.Parameters, "0.000000000001");
            hw.Parameters["VolatilityAdditionalParam"] = "0.000000000001"; //SetParam("VolatilityAdditionalParam", hw.Parameters, "0.000000000001");


            var cpi = EngineObjectTree.FindObject("CPI","Name"); // FindObjectName("CPI", EngineObjectTree);
            cpi.Parameters["sigma"] = "0.0000000000001";//SetParam("sigma", cpi.Parameters, "0.0000000000001", "Sigma");
            //TODO: we should set the mpr to mpr*sigmaold/sigmanew
            var awe = EngineObjectTree.FindObject("AWE", "Name");// FindObjectName("AWE", EngineObjectTree);
            awe.Parameters["sigma"] = "0.0000000000001"; //SetParam("sigma", awe.Parameters, "0.0000000000001", "Sigma");

            //Create one const vol model and set all other assumptions to correct mean return in model.
            //Find all models of type equity
            var modeList = EngineObjectTree.FindObjects("Model");
            var Models = EngineObjectTree.FindObject("Models");
            List<EngineObject> replacementEngineObjects = new List<EngineObject>();
            foreach (EngineObject model in modeList)
            {

                if (model.Parameters["Type"]?.ToString() == "EQUITY")
                {
                    var volID = model.Parameters["volatility_model_id"];// GetParam("VolatilityModelID", model.Parameters, "volatility_model_id");
                    var volModel = Models.FindObject(volID.ToString(),"ModelID");// modeList.Find(x => GetParam("ModelID", x.Parameters, "model_id") == volID);
                    double expectedReturn = 0.0;
                    if (volModel.Parameters["Type"].ToString() == "REGIMESWITCHVOLATILITY")
                    {
                        var p12 = Double.Parse(volModel.Parameters["ProbabilityState1Stat2"].ToString());
                        var p21 = Double.Parse(volModel.Parameters["ProbabilityState2Stat1"].ToString());
                        var mu1 = Double.Parse(volModel.Parameters["MeanReturnState1"].ToString());
                        var mu2 = Double.Parse(volModel.Parameters["MeanReturnState2"].ToString());
                        var sigma1 = Double.Parse(volModel.Parameters["VolatilityState1"].ToString());
                        var sigma2 = Double.Parse(volModel.Parameters["VolatilityState2"].ToString());

                        var prob2 = (p12) / (p12 + p21);
                        var prob1 = 1 - prob2;

                        expectedReturn = prob1 * (mu1 - 0.5 * sigma1 * sigma1) + prob2 * (mu2 - 0.5 * sigma2 * sigma2);
                    }
                    else
                    {
                        expectedReturn = Double.Parse(volModel.Parameters["mean_return"].ToString()) - 0.5*Math.Pow(Double.Parse(volModel.Parameters["volatility"].ToString()),2);
                    }
                    expectedReturn = Math.Round(expectedReturn, 8);
                    var tempmodel = new EngineObject()
                    {
                        Name = model.Name,
                        NodeName = model.NodeName,
                        Children = new List<EngineObject>(),
                        Parameters = new ParamList()
                    };
                    tempmodel.Parameters.Add(new Parameter() {Name="Type",Value="DETERMINISTICEQUITY"});
                    tempmodel.Parameters.Add(new Parameter() { Name = "ModelID", Value = model.Parameters["ModelID"]});
                    tempmodel.Parameters.Add(new Parameter() { Name = "Name", Value = model.Parameters["Name"] });
                    tempmodel.Parameters.Add(new Parameter() { Name = "UseNominalRates", Value = model.Parameters["UseNominalRates"]});
                    tempmodel.Parameters.Add(new Parameter() { Name = "NominalRatesModelID", Value = model.Parameters["NominalRatesModelID"]});
                    tempmodel.Parameters.Add(new Parameter() { Name = "MeanReturn", Value = expectedReturn.ToString()});
                    tempmodel.Parameters.Add(new Parameter() { Name = "IncomeModelID", Value = "0" });
                    replacementEngineObjects.Add(tempmodel);                                        
                }
            }

            List<EngineObject> removeList = new List<EngineObject>();

            foreach (EngineObject child in Models.Children)
            {
                var mtype = child.Parameters["Type"].ToString(); // GetParam("Type", child.Parameters);
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

            var Corr = EngineObjectTree.FindObject("Correlations");// FindObjectNodeName("Correlations", EngineObjectTree);
            Corr.Children.Clear();
        }



        public void Removealloutputs()
        {
            EngineObjectTree.FindObject("Queries").Children.Clear();
            EngineObjectTree.FindObject("Operators").Children.Clear();
        }

        public void AddtransactionLog(string outputfilename, int[] scenarios)
        {
            EngineObject tlog_object = new EngineObject() { Name = "TransactionLog", NodeName = "TransactionLog", Children = new List<EngineObject>(), Parameters = new ParamList() };
            tlog_object.Parameters.Add(new Parameter() { Name = "LogFile", Value = outputfilename });
            foreach (int scenario in scenarios)
            {
                EngineObject scenarios_object = new EngineObject() { Name = "Scenarios", NodeName = "Scenarios", Children = new List<EngineObject>(), Parameters = new ParamList() };
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
                temp.Add(new Model() {Name  =  node.Parameters["Name"].ToString(), ID = node.Parameters["ModelID"].ToString(), Type = node.Parameters["Class"].ToString()});
            }
            return temp;
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
                    return param.Value.ToString();
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
                            return param.Value.ToString();
                        }
                    }
                }
            }
            return null;
        }


        private EngineObject newQueryFilter(string Field, string Value, string ObjectName)
        {

            EngineObject QueryFilterCriter1 = new EngineObject() { Name = "QueryFilterCriteria", NodeName = "QueryFilterCriteria", Parameters = new ParamList() };
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Field", Value = Field });
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Value", Value = Value });
            EngineObject Where = new EngineObject() { Name = "Where", NodeName = "Where", Children = new List<EngineObject>(), Parameters = new ParamList() };
            Where.Children.Add(QueryFilterCriter1);
            EngineObject QueryFilter = new EngineObject() { Name = "QueryFilter", NodeName = "QueryFilter", Children = new List<EngineObject>(), Parameters = new ParamList() };
            QueryFilter.Children.Add(Where);
            QueryFilter.Parameters.Add(new Parameter() { Name = "ObjectName", Value = ObjectName });

            return QueryFilter;
        }



        private EngineObject Value(string Valuetype)
        {

            EngineObject ValueTypes = new EngineObject() { Name = "ValueTypes", NodeName = "ValueTypes", Children = new List<EngineObject>(), Parameters = new ParamList() };
            ValueTypes.Parameters.Add(new Parameter() { Name = "ValueType", Value = Valuetype });

            EngineObject Values = new EngineObject() { Name = "Values", NodeName = "Values", Children = new List<EngineObject>(), Parameters = new ParamList() };
            Values.Children.Add(ValueTypes);

            return Values;
        }

        private EngineObject newProductQuery(string QueryID, string productName, string taxWrapper, string ValueType)
        {
            EngineObject QueryFilter1 = newQueryFilter("Name", productName, "product");
            EngineObject QueryFilter2 = newQueryFilter("Name", taxWrapper, "tax_wrapper");

            EngineObject Filter = new EngineObject() { Name = "Filter", NodeName = "Filter", Children = new List<EngineObject>(), Parameters = new ParamList() };
            Filter.Children.Add(QueryFilter1);
            Filter.Children.Add(QueryFilter2);

            EngineObject Query = new EngineObject() { Name = "Query", NodeName = "Query", Children = new List<EngineObject>(), Parameters = new ParamList() };
            Query.Parameters.Add(new Parameter() { Name = "QueryID", Value = QueryID });
            Query.Parameters.Add(new Parameter() { Name = "Type", Value = "SIMVALUE" });
            Query.Children.Add(Value(ValueType));

            Query.Children.Add(Filter);
            return Query;
        }


        private EngineObject newModelQuery(string QueryID, string modelID, string ValueType)
        {
            EngineObject QueryFilter1 = newQueryFilter("ModelID", modelID, "model");

            EngineObject Filter = new EngineObject() { Name = "Filter", NodeName = "Filter", Children = new List<EngineObject>(), Parameters = new ParamList() };
            Filter.Children.Add(QueryFilter1);


            EngineObject Query = new EngineObject() { Name = "Query", NodeName = "Query", Children = new List<EngineObject>(), Parameters = new ParamList() };
            Query.Parameters.Add(new Parameter() { Name = "QueryID", Value = QueryID });
            Query.Parameters.Add(new Parameter() { Name = "Type", Value = "SIMVALUE" });
            Query.Children.Add(Value(ValueType));

            Query.Children.Add(Filter);
            return Query;
        }


        private EngineObject newOperator(string operatorID, string queryID, string valuetype, string timestepstart, string timestepend)
        {
            EngineObject OperationApplyTo = new EngineObject() {Name= "OperationApplyTo", NodeName = "OperationApplyTo", Children = new List<EngineObject>(), Parameters = new ParamList()};
            OperationApplyTo.Parameters.Add(new Parameter() {Name= "QueryID", Value=queryID});
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "Value", Value = valuetype });
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "TimeStepStart", Value = timestepstart });
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "TimeStepEnd", Value = timestepend });

            EngineObject ApplyTo = new EngineObject() {Name="ApplyTo",NodeName = "ApplyTo", Children = new List<EngineObject>(), Parameters = new ParamList()};
            ApplyTo.Children.Add(OperationApplyTo);

            EngineObject Operation = new EngineObject() { Name = "Operation", NodeName = "Operation", Children = new List<EngineObject>(), Parameters = new ParamList() };
            Operation.Children.Add(ApplyTo);
            Operation.Parameters.Add(new Parameter() {Name="Name",Value= "Scenarios" });
            Operation.Parameters.Add(new Parameter() { Name = "Type", Value = "SCENARIOALL" });

            EngineObject Operator = new EngineObject() { Name = "Operator", NodeName = "Operator", Children = new List<EngineObject>(), Parameters = new ParamList() };
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
        
        private EngineObject economicModel(List<EngineObject> models, string modelName, string currency, string calibration_type)
        {
            var tempo = new EngineObject() {Name="economic_model",NodeName = "economic_model", Children = new List<EngineObject>(), Parameters = new ParamList()};
            tempo.Parameters.Add(new Parameter() {Name = "model_name", Value=modelName});
            tempo.Parameters.Add(new Parameter() { Name = "currency", Value = currency});
            tempo.Parameters.Add(new Parameter() { Name = "current_mean_return", Value = "0"});
            var calType = new EngineObject() {Name="types", NodeName = "types", Parameters = new ParamList(), Children = new List<EngineObject>()};
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


            string EffectiveDate = EngineObjectTree.FindObject("Params").Parameters["EffectiveDate"].ToString();
            var Models = FindObjectNodeName("Models", EngineObjectTree);

            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            


            //doc.AppendChild(processModel(doc, rootnode));

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
