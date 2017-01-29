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

        private void buildTree()
        {
            _engineObjectTree.Name = xmlDoc.Name;
            _engineObjectTree.Children = processChildren(xmlDoc);
            _engineObjectTree.Parameters = processParameters(xmlDoc);

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
                    temp.Add(new EngineObject {Name = child.Name, Children = processChildren(child), Parameters = processParameters(child) });          
                }               
            }
            return temp;
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
