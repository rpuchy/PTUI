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
using System.Diagnostics;

namespace RequestRepresentation
{
    class FileOpsImplementation : BaseFileOps
    {   //
        private EngineObject _engineObjectTree = new EngineObject();
        private Dictionary<string, string[]> OutputTypeMap = new Dictionary<string, string[]>();
     
        public XmlDocument xmlDoc;
        
        
        public FileOpsImplementation(string filename)
        {
            FileName = filename;
            BuildValuetypeDictionary();
            LoadFile(FileName);            
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
                    _engineObjectTree = buildTree(_xmlDoc);
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
            return SaveAs(FileName);
        }

        public bool SaveAs(string path)
        {
            FileName = path; //update the file that we point to.
            XmlDocument _xmldoc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = _xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = _xmldoc.DocumentElement;
            _xmldoc.InsertBefore(xmlDeclaration, root);

            _xmldoc.AppendChild(processModel(_xmldoc, EngineObjectTree));

            _xmldoc.Save(path);
            return true;
        }

        private static XmlElement processModel(XmlDocument doc, EngineObject node)
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
        

        public static bool Save(string path, EngineObjectViewModel rootnode)
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

        private static XmlElement processModel(XmlDocument doc, EngineObjectViewModel node)
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

        public static EngineObject buildTree(XmlDocument xmlDoc)
        {
            EngineObject temp = new EngineObject();
            temp.Name = xmlDoc.LastChild.Name;
            temp.NodeName = xmlDoc.LastChild.Name;
            temp.AddChildren(processChildren(xmlDoc.LastChild)); //we use the first child because the main node is the document header
            temp.Parameters = processParameters(xmlDoc.LastChild);
            return temp;
        }

        //checks if the _node has any non parameter children
        private static bool hasChildren(XmlNode _node)
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

        private static ParamList processParameters(XmlNode _node)
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

        private static string getName(XmlNode _node)
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

        private static bool isParameter(XmlNode _node)
        {
            return (_node.ChildNodes.Count == 1 && _node.FirstChild.Name == "#text");
        }

        private static bool isComment(XmlNode _node)
        {
            return (_node.Name == "#comment");
        }


        private static List<EngineObject> processChildren(XmlNode _node)
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
                    var tempo = new EngineObject
                    {
                        Name = getName(child),
                        NodeName = child.Name,                        
                        Parameters = processParameters(child)
                    };
                    tempo.AddChildren(processChildren(child));
                    temp.Add(tempo);
                }
            }
            return temp;
        }


        private void ChangeScenarioFileLocation(string inCalibrationFile, string outCalibrationFile, string newLocation, string prefix = "")
        {
            //first we need to change the output location of the scenario files
            XmlDocument CalibXml = new XmlDocument();
            CalibXml.Load(inCalibrationFile);
            string xPathQuery = "//ScenarioFiles//FileName";
            XmlNodeList temp = CalibXml.SelectNodes(xPathQuery);
            foreach (XmlNode _node in temp)
            {
                _node.InnerText = newLocation + prefix + Path.GetFileName(_node.InnerText);
            }

            CalibXml.Save(outCalibrationFile);



        }


        private Dictionary<string, double> CreateScenarioFiles(string simfile)
        {
            //this method will take in a sim file
            //Add scenario file outputs then process the outputs

            XmlDocument doc = new XmlDocument();
            doc.Load(simfile);
            //set all equity models to total return
            string  xPathQuery = "//Model[Type=\"EQUITY\"]//use_nominal_rates";
            XmlNodeList temp = doc.SelectNodes(xPathQuery);

            foreach (XmlNode node in temp)
            {
                node.InnerText = "false";
            }

            doc.SelectSingleNode("//Scenarios").InnerText = "30000";

            doc.SelectSingleNode("//time_steps").InnerText = "30";


            //Get all model ID's
            xPathQuery = "//Model[Type=\"EQUITY\"]//ModelID";
             temp = doc.SelectNodes(xPathQuery);

            XmlNode ScenarioFiles = doc.SelectSingleNode("//ScenarioFiles");
            ScenarioFiles.InnerText = "";

            string outputlocation = System.IO.Path.GetTempPath() ;

            //now add the scenario files
            foreach (XmlNode node in temp)
            {
                XmlElement ScenarioFile = doc.CreateElement(string.Empty, "ScenarioFile", string.Empty);

                XmlElement ModelId = doc.CreateElement(string.Empty, "ModelId", string.Empty);
                ModelId.AppendChild(doc.CreateTextNode(node.InnerText));
                ScenarioFile.AppendChild(ModelId);

                XmlElement Filename = doc.CreateElement(string.Empty, "FileName", string.Empty);
                Filename.AppendChild(doc.CreateTextNode(outputlocation+"Scenario_"+ node.InnerText+".csv"));
                ScenarioFile.AppendChild(Filename);

                XmlElement Valuetypes = doc.CreateElement(string.Empty, "ValueTypes", string.Empty);

                XmlElement valueType = doc.CreateElement(string.Empty, "ValueType", string.Empty);
                valueType.AppendChild(doc.CreateTextNode("ROLLUP"));
                Valuetypes.AppendChild(valueType);

                ScenarioFile.AppendChild(Valuetypes);
                ScenarioFiles.AppendChild(ScenarioFile);
            }


            string outpath = System.IO.Path.GetTempPath() + "\\scendata.xml";

            doc.Save(outpath);

            string UnitTestHarness = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\UnitTestHarness.exe";


            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = @"--forceoutput --testdata """ + outpath + @" "" --compdata c:\res.csv --csvresdata """ + outputlocation + @"\result.csv""";
            
            // Enter the executable to run, including the complete path
            start.FileName = UnitTestHarness;
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = true;
            int exitCode;
            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();

                // Retrieve the app's exit code
                exitCode = proc.ExitCode;
            }

            Dictionary<string, double> tempres = new Dictionary<string, double>();

            foreach (XmlNode node in temp)
            {
                tempres.Add(node.InnerText, CalcMedian(outputlocation + "Scenario_" + node.InnerText + ".csv",30));
            }

            return tempres;
            
        }


        private double CalcMedian(string filename, int timestep)
        {
            List<double> ScenarioData = new List<double>();
            Double[,] Result = new Double[2, 50];
            using (var fs = File.OpenRead(filename))
            using (var reader = new StreamReader(fs))
            {
                //skip the first line
                var header = reader.ReadLine().Split(',');
                //Figure out the index to use
                int headertouse = 0;
                for (int j = 0; j < header.Length; j++)
                {
                    if (header[j] == "ROLLUP")
                    {
                        headertouse = j;
                    }

                }
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (int.Parse(values[1]) == timestep)
                    {
                        var val = Math.Log(double.Parse(values[headertouse]))/timestep;
                        ScenarioData.Add(val);
                    }
                }
                //now process the data.
                ScenarioData.Sort();
            }
            return ScenarioData[(int)ScenarioData.Count / 2];
        }


        
        //returns the ouput component
        public void AddAlloutputs(int timestepstart, int timestepend, string logLocation)
        {
            //We follow the following process
            //1. we cycle through the ESG models and pick out all the models 
            //2. we cycle through the products and pick out the products
            //first remove all the products
            Removealloutputs();

            //Add the transactions log, with scenario 1

            // need to produce scenario files so we can produce the annualised excess return values for the various models

            string outpath = System.IO.Path.GetTempPath() + "\\temp.xml";

            SaveAs(outpath);

            Dictionary<string,double> medians = CreateScenarioFiles(outpath);


            AddtransactionLog(logLocation, new int[] {1});

            var Params = FindObjectNodeName("Params", EngineObjectTree);

            Params.Parameters["Scenarios"] = 1.ToString(); 

            Params.Parameters["InflationAdjusted"] = "false";

            Params.Parameters["output_file"] = "c:\\Foresight\\results\\results.csv"; 

            //convert ESG to deterministic
            ConvertToDeterministic(medians);

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



        private void ConvertToDeterministic(Dictionary<string,double> values)
        {
            var hw = EngineObjectTree.FindObject("AUYieldCurve", "Name").FindObject("ModelParameters");
            hw.Parameters["VolatilityShortRate"] = "0.000000000001";
            hw.Parameters["VolatilityAdditionalParam"] = "0.000000000001";


            var cpi = EngineObjectTree.FindObject("CPI", "Name");
            cpi.Parameters["sigma"] = "0.0000000000001";
            //TODO: we should set the mpr to mpr*sigmaold/sigmanew
            var awe = EngineObjectTree.FindObject("AWE", "Name");
            awe.Parameters["sigma"] = "0.0000000000001";

            //Create one const vol model and set all other assumptions to correct mean return in model.
            //Find all models of type equity
            var modeList = EngineObjectTree.FindObjects("Model");
            var Models = EngineObjectTree.FindObject("Models");
            List<EngineObject> replacementEngineObjects = new List<EngineObject>();
            foreach (EngineObject model in modeList)
            {

                if (model.Parameters["Type"]?.ToString() == "EQUITY")
                {
                    var volID = model.Parameters["volatility_model_id"];
                    var volModel = Models.FindObject(volID.ToString(), "ModelID");
                    double expectedReturn = 0.0;
                    if (volModel.Parameters["Type"].ToString() == "REGIMESWITCHVOLATILITY")
                    {
                        var p12 = Double.Parse(volModel.Parameters["ProbabilityState1State2"].ToString());
                        var p21 = Double.Parse(volModel.Parameters["ProbabilityState2State1"].ToString());
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
                        expectedReturn = Double.Parse(volModel.Parameters["mean_return"].ToString()) - 0.5 * Math.Pow(Double.Parse(volModel.Parameters["volatility"].ToString()), 2);
                    }
                    expectedReturn = Math.Round(values[ model.Parameters["ModelID"].ToString()], 8);
                    var tempmodel = new EngineObject()
                    {
                        Name = model.Name,
                        NodeName = model.NodeName
                    };
                    tempmodel.Parameters.Add(new Parameter() { Name = "Type", Value = "DETERMINISTICEQUITY" });
                    tempmodel.Parameters.Add(new Parameter() { Name = "ModelID", Value = model.Parameters["ModelID"] });
                    tempmodel.Parameters.Add(new Parameter() { Name = "Name", Value = model.Parameters["Name"] });
                    tempmodel.Parameters.Add(new Parameter() { Name = "UseNominalRates", Value = model.Parameters["UseNominalRates"] });
                    tempmodel.Parameters.Add(new Parameter() { Name = "NominalRatesModelID", Value = model.Parameters["NominalRatesModelID"] });
                    tempmodel.Parameters.Add(new Parameter() { Name = "MeanReturn", Value = expectedReturn.ToString() });
                    tempmodel.Parameters.Add(new Parameter() { Name = "IncomeModelID", Value = "0" });
                    replacementEngineObjects.Add(tempmodel);
                }
            }

            List<EngineObject> removeList = new List<EngineObject>();

            foreach (EngineObject child in Models.Children)
            {
                var mtype = child.Parameters["Type"]?.ToString();
                if (mtype == "EQUITY" || mtype == "REGIMESWITCHVOLATILITY" || mtype == "CONSTANTVOLATILITY")
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

            var Corr = EngineObjectTree.FindObject("Correlations");
            Corr.Children.Clear();
        }


        private void ConvertToDeterministic()
        {
            var hw = EngineObjectTree.FindObject("AUYieldCurve","Name").FindObject("ModelParameters");
            hw.Parameters["VolatilityShortRate"] = "0.000000000001"; 
            hw.Parameters["VolatilityAdditionalParam"] = "0.000000000001"; 


            var cpi = EngineObjectTree.FindObject("CPI","Name");
            cpi.Parameters["sigma"] = "0.0000000000001";
            //TODO: we should set the mpr to mpr*sigmaold/sigmanew
            var awe = EngineObjectTree.FindObject("AWE", "Name");
            awe.Parameters["sigma"] = "0.0000000000001"; 

            //Create one const vol model and set all other assumptions to correct mean return in model.
            //Find all models of type equity
            var modeList = EngineObjectTree.FindObjects("Model");
            var Models = EngineObjectTree.FindObject("Models");
            List<EngineObject> replacementEngineObjects = new List<EngineObject>();
            foreach (EngineObject model in modeList)
            {

                if (model.Parameters["Type"]?.ToString() == "EQUITY")
                {
                    var volID = model.Parameters["volatility_model_id"];
                    var volModel = Models.FindObject(volID.ToString(),"ModelID");
                    double expectedReturn = 0.0;
                    if (volModel.Parameters["Type"].ToString() == "REGIMESWITCHVOLATILITY")
                    {
                        var p12 = Double.Parse(volModel.Parameters["ProbabilityState1State2"].ToString());
                        var p21 = Double.Parse(volModel.Parameters["ProbabilityState2State1"].ToString());
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
                        NodeName = model.NodeName
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
                var mtype = child.Parameters["Type"]?.ToString(); 
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

            var Corr = EngineObjectTree.FindObject("Correlations");
            Corr.Children.Clear();
        }



        public void Removealloutputs()
        {
            EngineObjectTree.FindObject("Queries").Children.Clear();
            EngineObjectTree.FindObject("Operators").Children.Clear();
        }

        public void AddtransactionLog(string outputfilename, int[] scenarios)
        {
            EngineObject tlog_object = new EngineObject() { Name = "TransactionLog", NodeName = "TransactionLog" };
            tlog_object.Parameters.Add(new Parameter() { Name = "LogFile", Value = outputfilename });
            foreach (int scenario in scenarios)
            {
                EngineObject scenarios_object = new EngineObject() { Name = "Scenarios", NodeName = "Scenarios"};
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
                //get model type or class

                var type = node.Parameters["Type"]?.ToString();
                if (type == null)
                {
                    type = node.Parameters["Class"]?.ToString();
                }

                temp.Add(new Model() {Name  =  node.Parameters["Name"].ToString(), ID = node.Parameters["ModelID"].ToString(), Type = type});
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

            EngineObject QueryFilterCriter1 = new EngineObject() { Name = "QueryFilterCriteria", NodeName = "QueryFilterCriteria" };
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Field", Value = Field });
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Value", Value = Value });
            EngineObject Where = new EngineObject() { Name = "Where", NodeName = "Where" };
            Where.Children.Add(QueryFilterCriter1);
            EngineObject QueryFilter = new EngineObject() { Name = "QueryFilter", NodeName = "QueryFilter" };
            QueryFilter.Children.Add(Where);
            QueryFilter.Parameters.Add(new Parameter() { Name = "ObjectName", Value = ObjectName });

            return QueryFilter;
        }



        private EngineObject Value(string Valuetype)
        {

            EngineObject ValueTypes = new EngineObject() { Name = "ValueTypes", NodeName = "ValueTypes"};
            ValueTypes.Parameters.Add(new Parameter() { Name = "ValueType", Value = Valuetype });

            EngineObject Values = new EngineObject() { Name = "Values", NodeName = "Values"};
            Values.Children.Add(ValueTypes);

            return Values;
        }

        private EngineObject newProductQuery(string QueryID, string productName, string taxWrapper, string ValueType)
        {
            EngineObject QueryFilter1 = newQueryFilter("Name", productName, "product");
            EngineObject QueryFilter2 = newQueryFilter("Name", taxWrapper, "tax_wrapper");

            EngineObject Filter = new EngineObject() { Name = "Filter", NodeName = "Filter" };
            Filter.Children.Add(QueryFilter1);
            Filter.Children.Add(QueryFilter2);

            EngineObject Query = new EngineObject() { Name = "Query", NodeName = "Query" };
            Query.Parameters.Add(new Parameter() { Name = "QueryID", Value = QueryID });
            Query.Parameters.Add(new Parameter() { Name = "Type", Value = "SIMVALUE" });
            Query.Children.Add(Value(ValueType));

            Query.Children.Add(Filter);
            return Query;
        }


        private EngineObject newModelQuery(string QueryID, string modelID, string ValueType)
        {
            EngineObject QueryFilter1 = newQueryFilter("ModelID", modelID, "model");

            EngineObject Filter = new EngineObject() { Name = "Filter", NodeName = "Filter"};
            Filter.Children.Add(QueryFilter1);


            EngineObject Query = new EngineObject() { Name = "Query", NodeName = "Query" };
            Query.Parameters.Add(new Parameter() { Name = "QueryID", Value = QueryID });
            Query.Parameters.Add(new Parameter() { Name = "Type", Value = "SIMVALUE" });
            Query.Children.Add(Value(ValueType));

            Query.Children.Add(Filter);
            return Query;
        }


        private EngineObject newOperator(string operatorID, string queryID, string valuetype, string timestepstart, string timestepend)
        {
            EngineObject OperationApplyTo = new EngineObject() {Name= "OperationApplyTo", NodeName = "OperationApplyTo"};
            OperationApplyTo.Parameters.Add(new Parameter() {Name= "QueryID", Value=queryID});
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "Value", Value = valuetype });
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "TimeStepStart", Value = timestepstart });
            OperationApplyTo.Parameters.Add(new Parameter() { Name = "TimeStepEnd", Value = timestepend });

            EngineObject ApplyTo = new EngineObject() {Name="ApplyTo",NodeName = "ApplyTo"};
            ApplyTo.Children.Add(OperationApplyTo);

            EngineObject Operation = new EngineObject() { Name = "Operation", NodeName = "Operation" };
            Operation.Children.Add(ApplyTo);
            Operation.Parameters.Add(new Parameter() {Name="Name",Value= "Scenarios" });
            Operation.Parameters.Add(new Parameter() { Name = "Type", Value = "SCENARIOALL" });

            EngineObject Operator = new EngineObject() { Name = "Operator", NodeName = "Operator" };
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

            queries.AddChild(QueryPart);
            operators.AddChild(OperatorPart);

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
            var tempo = new EngineObject() {Name="economic_model",NodeName = "economic_model"};
            tempo.Parameters.Add(new Parameter() {Name = "model_name", Value=modelName});
            tempo.Parameters.Add(new Parameter() { Name = "currency", Value = currency});
            tempo.Parameters.Add(new Parameter() { Name = "current_mean_return", Value = "0"});
            var calType = new EngineObject() {Name="types", NodeName = "types"};
            calType.Parameters.Add(new Parameter() {Name = "calibration_type",Value=calibration_type});
            tempo.AddChild(calType);
            foreach (var engineObject in models)
            {
                tempo.AddChild(engineObject);
            }
            return tempo;
        }

        public static void CalibrationsTemplate(string outputfilename, List<CalibData> calibrationData)
        {

            List<EngineObject> calibObjects = new List<EngineObject>();

            foreach (CalibData data in calibrationData)
            {
                XmlDocument _xmlDoc = new XmlDocument();
                _xmlDoc.Load(data.CalibrationFile);
                calibObjects.Add(buildTree(_xmlDoc));
            }
            
            DateTime EffectiveDate = DateTime.Parse(calibObjects[0].FindObject("Params").Parameters["EffectiveDate"].ToString());
            
            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement xmlCalibrations = doc.CreateElement(string.Empty, "Calibrations", string.Empty);
            doc.AppendChild(xmlCalibrations);

            XmlElement xmlCalibrationTemplates = doc.CreateElement(string.Empty, "CalibrationTemplate", string.Empty);
            xmlCalibrations.AppendChild(xmlCalibrationTemplates);

            XmlElement xmlEffectiveDate = doc.CreateElement(string.Empty, "EffectiveDate", string.Empty);
            xmlCalibrationTemplates.AppendChild(xmlEffectiveDate);
            xmlEffectiveDate.AppendChild(doc.CreateTextNode(EffectiveDate.ToString("O").Substring(0, EffectiveDate.ToString("O").Length-4)));

            XmlElement xmlTemplateGroup = doc.CreateElement(string.Empty, "TemplateGroup", string.Empty);
            xmlCalibrationTemplates.AppendChild(xmlTemplateGroup);
            xmlTemplateGroup.AppendChild(doc.CreateTextNode("AMP"));

            for (int i = 0; i < calibObjects.Count; i++)
            {

                var Models = FindObjectNodeName("Models", calibObjects[i]);

                XmlElement xmleconomicmodels = doc.CreateElement(string.Empty, "economic_models", string.Empty);
                xmlCalibrationTemplates.AppendChild(xmleconomicmodels);

                XmlElement xmlassumptionSet = doc.CreateElement(string.Empty, "assumption_set", string.Empty);
                xmleconomicmodels.AppendChild(xmlassumptionSet);
                xmlassumptionSet.AppendChild(doc.CreateTextNode(calibrationData[i].AssumptionSet));

                XmlElement xmlEffectiveDate2 = doc.CreateElement(string.Empty, "effective_date", string.Empty);
                xmleconomicmodels.AppendChild(xmlEffectiveDate2);
                xmlEffectiveDate2.AppendChild(doc.CreateTextNode(EffectiveDate.ToString("dd/MM/yyyy")));

                //This is where we cycle models.

                foreach (var model in Models.Children)
                {

                    string type = model.Parameters["Type"]?.ToString();

                    if (type == null)
                    {
                        type = model.Parameters["Class"].ToString();
                    }

                    if (type == "EQUITY" || type == "CHILDEQUITY" || type == "TWOFACTORHULLWHITE" ||
                        type == "DETERMINISTICEQUITY" || type == "ORNSTEINUHLENBECK")
                    {
                        //we only add the main model as the link then add the dependencies
                        XmlElement xmleconomicmodel = doc.CreateElement(string.Empty, "economic_model", string.Empty);
                        xmleconomicmodels.AppendChild(xmleconomicmodel);

                        string modelname = "";
                        string calibrationname = "";
                        if (type == "TWOFACTORHULLWHITE")
                        {
                            modelname = "AustralianYieldCurve";
                            calibrationname = modelname.ToUpper() + "-2FHW";
                        }
                        else
                        {
                            modelname = model.Parameters["Name"].ToString().Replace(" ", string.Empty);
                            calibrationname = modelname;
                        }

                        //Now are add the type of the model
                        if (type == "DETERMINISTICEQUITY")
                        {
                            calibrationname = modelname + "-DETERMINISTIC";
                        }

                        if ((type == "TWOFACTORHULLWHITE") &&
                            (Double.Parse(
                                 model.FindObject("ModelParameters").Parameters["VolatilityShortRate"].ToString()) <
                             0.000001)
                            &&
                            (Double.Parse(
                                 model.FindObject("ModelParameters").Parameters["VolatilityAdditionalParam"].ToString()) <
                             0.000001))
                        {
                            calibrationname = modelname + "-DETERMINISTIC";
                        }

                        if (type == "EQUITY")
                        {
                            //identify the type of the vol model
                            string voltype =
                                Models.FindObject(model.Parameters["VolatilityModelID"].ToString(), "ModelID")
                                    .Parameters[
                                        "Type"].ToString();

                            calibrationname = modelname + "-" + voltype;
                        }

                        if (type == "ORNSTEINUHLENBECK")
                        {
                            calibrationname = modelname + "-" + type;
                            if (Double.Parse(model.Parameters["Sigma"].ToString()) < 0.00001)
                            {
                                calibrationname = calibrationname + "-DETERMINISTIC";
                            }
                        }

                        XmlElement xmlModelName = doc.CreateElement(string.Empty, "model_name", string.Empty);
                        xmleconomicmodel.AppendChild(xmlModelName);
                        xmlModelName.AppendChild(doc.CreateTextNode(modelname));

                        XmlElement xmlCurrency = doc.CreateElement(string.Empty, "currency", string.Empty);
                        xmleconomicmodel.AppendChild(xmlCurrency);
                        xmlCurrency.AppendChild(doc.CreateTextNode(calibrationData[i].Currency));

                        XmlElement xmlcurrentmeanreturn = doc.CreateElement(string.Empty, "current_mean_return",
                            string.Empty);
                        xmleconomicmodel.AppendChild(xmlcurrentmeanreturn);
                        xmlcurrentmeanreturn.AppendChild(doc.CreateTextNode("0"));

                        XmlElement xmltypes = doc.CreateElement(string.Empty, "types", string.Empty);
                        xmleconomicmodel.AppendChild(xmltypes);

                        XmlElement xmlcalibrationtype = doc.CreateElement(string.Empty, "calibration_type", string.Empty);
                        xmltypes.AppendChild(xmlcalibrationtype);

                        xmlcalibrationtype.AppendChild(doc.CreateTextNode(calibrationname));

                        XmlElement xmlmodels = doc.CreateElement(string.Empty, "models", string.Empty);
                        xmleconomicmodel.AppendChild(xmlmodels);

                        //Add the model, it's volatility model and it's income model if they exists
                        EngineObject volmodel = null;
                        if (model.Parameters["VolatilityModelID"] != null &&
                            model.Parameters["VolatilityModelID"].ToString() != "0")
                        {
                            volmodel = Models.FindObject(model.Parameters["VolatilityModelID"].ToString(), "ModelID");
                        }

                        EngineObject incomemodel = null;
                        if (model.Parameters["IncomeModelID"] != null &&
                            model.Parameters["IncomeModelID"].ToString() != "0")
                        {
                            incomemodel = Models.FindObject(model.Parameters["IncomeModelID"].ToString(), "ModelID");
                        }

                        xmlmodels.AppendChild(processModel(doc, model));
                        if (volmodel != null)
                        {
                            xmlmodels.AppendChild(processModel(doc, volmodel));
                        }
                        if (incomemodel != null)
                        {
                            xmlmodels.AppendChild(processModel(doc, incomemodel));
                        }

                    }
                }
                //Now we add the correlations
                EngineObject correlations = calibObjects[i].FindObject("Correlations");
                XmlElement xmlCorrelations = doc.CreateElement(string.Empty, "Correlations", string.Empty);
                xmlCalibrationTemplates.AppendChild(xmlCorrelations);

                XmlElement xmlAssumptionsSet2 = doc.CreateElement(string.Empty, "assumption_set", string.Empty);
                xmlCorrelations.AppendChild(xmlAssumptionsSet2);

                xmlAssumptionsSet2.AppendChild(doc.CreateTextNode(calibrationData[i].AssumptionSet));

                XmlElement xmlEffectivedate3 = doc.CreateElement(string.Empty, "effective_date", string.Empty);
                xmlCorrelations.AppendChild(xmlEffectivedate3);

                xmlEffectivedate3.AppendChild(doc.CreateTextNode(EffectiveDate.ToString("dd/MM/yyyy")));

                foreach (EngineObject child in correlations.Children)
                {
                    xmlCorrelations.AppendChild(processModel(doc, child));
                }
            }

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
