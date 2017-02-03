using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RequestRepresentation
{
    public class ConfigLoader
    {
        const string CONFIG                 = "config.xml";
        public const string BASEREQUEST     = "BaseRequest";
        public const string INPUTSCHEMADEF  = "InputSchemaDef";
        //
        FileOpsImplementation xmlObj;
        public XmlNode root;
        public Dictionary<string,string> GlobalConfig = null;
        //
        public ConfigLoader()
        {
            System.Diagnostics.Debug.WriteLine( "Loading config..." + 
                                                Environment.NewLine );
            xmlObj = new FileOpsImplementation("");
            xmlObj.ProcessFile( CONFIG );
            System.Diagnostics.Debug.WriteLine( "Config xml parsed.. " + 
                                                Environment.NewLine );
            GlobalConfig = new Dictionary<string, string>();
            root = xmlObj.xmlDoc.DocumentElement;
        }
        // expects flat config file
        public void parseConfig() {
            XmlNode conf = root.SelectSingleNode( "/config" );
            foreach ( XmlNode elem in conf ) {
                GlobalConfig.Add( elem.LocalName, elem.InnerText );
            }
        }
        //
        public int Count() {
            return GlobalConfig.Count;
        }

        public string getBaseRequest()
        {
            if ( GlobalConfig != null ) {
                return GlobalConfig[ BASEREQUEST ];
            }
            else {
                return ( root.SelectSingleNode( "/config/" + BASEREQUEST )
                       ).InnerText;
            }
        }
        public string getInputXDM()
        {
            if ( GlobalConfig != null ) {
                return GlobalConfig[ INPUTSCHEMADEF ];
            }
            else {
                return ( root.SelectSingleNode( "/config/" + INPUTSCHEMADEF )
                       ).InnerText;
            }
        }
        //
    }//end ConfigLoader
}
