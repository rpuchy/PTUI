using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics.PerformanceData;
using System.Windows;
using System.Xml;
using BusinessLib;
using TreeViewWithViewModelDemo.TextSearch;

namespace RequestRepresentation
{
    class FileOpsImplementation : BaseFileOps
    {   //
        public XmlDocument xmlDoc;
        private EngineObject _engineObjectTree = new EngineObject();
        //
        public FileOpsImplementation()
        {
            xmlDoc = new XmlDocument();
        }
        //
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

            foreach (XmlNode child in _node.ChildNodes)
            {
                if (!hasChildren(child)&&child.ChildNodes.Count>0)
                {
                    //if there are no children then it's a parameter
                    string val = child.FirstChild.Value;
                    string pname = child.Name;
                    temp.Add(new Parameter() {Name= pname,Value=val});
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
            return _node.Name;
        }


        private List<EngineObject> processChildren(XmlNode _node)
        {
            List<EngineObject> temp = new List<EngineObject>();

            foreach (XmlNode child in _node.ChildNodes)
            {
                //First Check if the child has more children or only text
                //1. children is only 1 
                //2. child is #text
                if (hasChildren(child))
                {                     
                    temp.Add(new EngineObject {Name = getName(child), NodeName= child.Name,  Children = processChildren(child), Parameters = processParameters(child) });          
                }               
            }
            return temp;
        }

        //returns the ouput component
        public EngineObject AddAlloutputs(EngineObject node)
        {


            //We follow the following process
            //1. we cycle through the ESG models and pick out all the models 
            //2. we cycle through the products and pick out the products
            foreach (var child in node.Children)
            {
                     
            }

        }



        private List<string> AddProducts(EngineObject node, string prev_taxwrapper)
        {
            var temp = new List<string>();
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
             
                temp = AddProducts(child,taxWrapper);
            }
            var outlist = new List<string>();
            foreach (var product in temp)
            {
                outlist.Add(product);
            }
            if (node.NodeName == "Product")
            {
                outlist.Add(taxWrapper+","+node.Name+","+getProductType(node.Parameters ));
            }
            return outlist;
        }

        private string getProductType(IList<Parameter> Parameters)
        {
            foreach (var param in Parameters)
            {
                if (param.Name == "Type")
                {
                    return param.Value;
                }

            }
            return null;
        }


        private EngineObject newQueryFilter(string Field, string Value, string ObjectName)
        {

            EngineObject QueryFilterCriter1 = new EngineObject() { Name = "QueryFilterCriteria", NodeName = "QueryFilterCriteria", Parameters = new List<Parameter>() };
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Field", Value = "" });
            QueryFilterCriter1.Parameters.Add(new Parameter() { Name = "Value", Value = "" });
            EngineObject Where = new EngineObject() { Name = "Where", NodeName = "Where", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Where.Children.Add(QueryFilterCriter1);
            EngineObject QueryFilter = new EngineObject() { Name = "QueryFilter", NodeName = "QueryFilter", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            QueryFilter.Children.Add(Where);
            QueryFilter.Parameters.Add(new Parameter() { Name = "ObjectName", Value = "" });

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

        private EngineObject newQuery(string QueryID, string productName, string taxWrapper, string ValueType)
        {
            EngineObject QueryFilter1 = newQueryFilter("Name", productName, "product");
            EngineObject QueryFilter2 = newQueryFilter("Name", taxWrapper, "tax_wrapper");

            EngineObject Filter = new EngineObject() { Name = "Filter", NodeName = "Filter", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Filter.Children.Add(QueryFilter1);
            Filter.Children.Add(QueryFilter2);

            EngineObject Query = new EngineObject() { Name = "Query", NodeName = "Query", Children = new List<EngineObject>(), Parameters = new List<Parameter>() };
            Query.Parameters.Add(new Parameter() { Name = "QueryID", Value = QueryID });
            Query.Parameters.Add(new Parameter() { Name = "Type", Value = ValueType });
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


        public EngineObject Addoutput(string productName, string valueType, string taxWrapper, string timestepstart, string timestepend, EngineObject node)
        {

            EngineObject QueryPart = newQuery("Query_" + productName + "_" + valueType, productName, taxWrapper,valueType);
            EngineObject OperatorPart = newOperator(productName + "_" + valueType,
                "Query_" + productName + "_" + valueType, valueType, timestepstart, timestepend);

            foreach (var child in node.Children)
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
            return node;

        }


        public EngineObject EngineObjectTree
        {
            get { return _engineObjectTree;}
            set { _engineObjectTree = value; }
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
