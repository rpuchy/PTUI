using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
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
        public XmlDocument xmlDoc;
        private EngineObject _engineObjectTree = new EngineObject();
        private Dictionary<string, string[]> OutputTypeMap = new Dictionary<string, string[]>();
        //
        public FileOpsImplementation()
        {
            xmlDoc = new XmlDocument();
            BuildValuetypeDictionary();
        }
        //


        private void BuildValuetypeDictionary()
        {
            OutputTypeMap.Clear();
            using (var fs = File.OpenRead(@"C:\Git\PTUI\UI\PlanningTool\PlanningTool\Valuetypes.csv"))
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

        public override void ProcessFile( string path )
        {
            try {
                using ( FileStream file_reader = new FileStream( path, FileMode.Open ) )
                using ( XmlReader reader = XmlReader.Create( file_reader ) )
                {
                    xmlDoc.Load( reader );
                    buildTree();
                }
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( "ERROR: " + ex.Message + ex.ToString() +
                                                    Environment.NewLine );
                throw;
            }
        }


        public bool SaveAs(string path)
        {
            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            doc.AppendChild(processModel(doc, EngineObjectTree));

            doc.Save(path);
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

        private void buildTree()
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
        public void AddAlloutputs()
        {


            //We follow the following process
            //1. we cycle through the ESG models and pick out all the models 
            //2. we cycle through the products and pick out the products

            var productlist = GetProducts(EngineObjectTree, "");
            var modeList = GetModels(EngineObjectTree);
            foreach (var prod in productlist)
            {
                string[] Valuetypes = OutputTypeMap[prod.Type];
                foreach (string vtype in Valuetypes)
                {
                    AddProductOutput(prod.Name, vtype, prod.TaxWrapper, 0.ToString(), 100.ToString());
                }               
            }
            foreach (var model in modeList)
            {
                string[] Valuetypes = OutputTypeMap[model.Type];
                foreach (string vtype in Valuetypes)
                {
                    AddModelOutput(model.ID ,model.Name, vtype, 0.ToString(), 100.ToString());
                }
            }


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

        private string GetParam(string ParamName, IList<Parameter> Parameters, string alternate= "")
        {
            var param = ((List<Parameter>) Parameters).Find(x => x.Name == ParamName);
            if (param == null&&alternate!="")
            {
                param = ((List<Parameter>)Parameters).Find(x => x.Name == alternate);
            }


            return (param == null)?  string.Empty : param.Value ;
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

            foreach (var child in EngineObjectTree.Children)
            {
                if (child.Name == "OutputRequirements")
                {
                    foreach (var child2 in child.Children)
                    {
                        if (child2.Name == "Queries")
                        {
                            child2.Children.Add(QueryPart);
                        }
                        if (child2.Name == "Operators")
                        {
                            child2.Children.Add(OperatorPart);
                        }
                    }
                }
            }            
        }

        private void AddModelOutput(string modelID, string Name, string valueType, string timestepstart, string timestepend)
        {

            EngineObject QueryPart = newModelQuery("Query_" + modelID + "_" + valueType, modelID, valueType);
            EngineObject OperatorPart = newOperator("Oper_" +modelID +"_"+ Name + "_" + valueType,
                "Query_" + modelID + "_" + valueType, valueType, timestepstart, timestepend);

            foreach (var child in EngineObjectTree.Children)
            {
                if (child.Name == "OutputRequirements")
                {
                    foreach (var child2 in child.Children)
                    {
                        if (child2.Name == "Queries")
                        {
                            child2.Children.Add(QueryPart);
                        }
                        if (child2.Name == "Operators")
                        {
                            child2.Children.Add(OperatorPart);
                        }
                    }
                }
            }
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
