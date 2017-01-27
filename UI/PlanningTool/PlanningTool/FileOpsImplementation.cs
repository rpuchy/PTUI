using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Xml;

namespace RequestRepresentation
{
    class FileOpsImplementation : BaseFileOps
    {   //
        public XmlDocument xmlDoc;
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
                }
            }
            catch( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( "ERROR: " + ex.Message + ex.ToString() +
                                                    Environment.NewLine );
                throw;
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
