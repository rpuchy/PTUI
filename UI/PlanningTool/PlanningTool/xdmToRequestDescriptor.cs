using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace RequestRepresentation
{
    //
    public class xdmToRequestDescriptor
    {
        // ASSUMPTIONS ABOUT XDM FILE STRUCTURE HERE
        const string CONTAINERATTRVALUE = "container";
        const string SIMULATIONTAG      = "Simulation";
        const string CONSTRAINTSLISTTAG = "ConstraintLists";
        const string KEYSEPARATOR       = "::";
        //
        const string KEYBINDER          = ",";
        //
        public XmlDocument xmlObj = null;
        public XmlNode root = null;
        //
        // Models
        // Correlations
        // Tables
        // Portfolios
        //      TaxWrappers
        //          Products
        //          Holdings
        //      RebalanceRules
        //      People
        //          holdings
        // OverrideSets
        // Queries
        // Operators
        // ScenarioFiles

        // contains object name and component part names
        public Dictionary<string,List<string>> RequestDescriptor;
        // contains type associations, object type to name
        public Dictionary<string,List<string>> TypeAssociations;
        // contains possible values / tag types the target element can reference
        public Dictionary<string,List<string>> ReferenceAssocs;
        // contains name/dependency lists
        // lists where target determines behavior of associated element
        //  ( enabled/disabled in UI and used or not used in engine )
        public Dictionary<string,List<string>> DependencyLists;
        // logical groupings or sub-groupings of elements
        public Dictionary<string,List<string>> Groups;
        // contains constraint rules by name
        public Dictionary<string,Constraint>   Constraints;
        // contains direct relationship with another field where target can 
        // search the types in its list
        public Dictionary<KeyValuePair<string,string>,List<string>> FieldRefs;
        // constains scopeTo syntax where target is xpath to target and range 
        // defines elements scoped to that target
        public Dictionary<string,List<string>> ScopeToList;
        //
        public xdmToRequestDescriptor( ref ConfigLoader conf )
        {
            System.Diagnostics.Debug.WriteLine( "xdmToRequestDescriptor::init..." + Environment.NewLine );
            xmlObj = new XmlDocument();
            System.Diagnostics.Debug.WriteLine( "Read file.. " + Environment.NewLine );
            if ( conf.GlobalConfig != null ) {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                XmlReader reader = XmlReader.Create( conf.getInputXDM()
                                                    ,settings );
                xmlObj.Load( reader );
            }
            System.Diagnostics.Debug.WriteLine( "File read and xml parsed.. " + Environment.NewLine );
            RequestDescriptor   = new Dictionary<string,List<string>>();
            TypeAssociations    = new Dictionary<string,List<string>>();
            ReferenceAssocs     = new Dictionary<string,List<string>>();
            DependencyLists     = new Dictionary<string,List<string>>();
            Groups              = new Dictionary<string,List<string>>();
            Constraints         = new Dictionary<string,Constraint>();
            FieldRefs           = new Dictionary<KeyValuePair<string,string>,List<string>>();
            ScopeToList         = new Dictionary<string,List<string>>();
            root = xmlObj.DocumentElement;
            System.Diagnostics.Debug.WriteLine( "xdmToRequestDescriptor::init Complete..." + Environment.NewLine );
        }
        //
        public void distiller()
        {   // pull out all object types into RequestDescriptor
            System.Diagnostics.Debug.WriteLine( "xdmToRequestDescriptor::distiller::init..." + Environment.NewLine );
            // process constraints first
            processConstraints();
            System.Diagnostics.Debug.WriteLine( "Parsed Constraints.. " + Environment.NewLine );
            // process simulation definition
            processSimulationObjects();
            System.Diagnostics.Debug.WriteLine( "Parsed Container lists" + Environment.NewLine );
            // debug print
            printAllTheThings();
            System.Diagnostics.Debug.WriteLine( "xdmToRequestDescriptor::distiller::init Complete..." + Environment.NewLine );
        }

        //
        //  I don't think I will ever be proud of this Constraint processing method
        //
        public void processConstraints()
        {
            XmlNode constraintRoot = root.SelectSingleNode( CONSTRAINTSLISTTAG );
            Constraint newCons;
            foreach ( XmlNode constraint in constraintRoot )
            {   // parse primitives
                System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
                System.Diagnostics.Debug.WriteLine( "Constraints definitions: " + constraint.InnerXml + Environment.NewLine );
                System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
                System.Diagnostics.Debug.WriteLine( "Constraint: " + constraint.InnerXml + Environment.NewLine );
                switch ( constraint.Name )
                {
                    case "bool":
                    case "Bool":
                    case "boolean":
                    case "Boolean": {
                        newCons = new Constraint( /* setToBool */ true );
                        //newCons.add( constraint.InnerText );
                        processConstraint( constraint.InnerText, ref newCons );
                        break;
                    }
                    case "char":
                    case "Char":
                    case "character":
                    case "Character": {
                        newCons = new Constraint( constraint.Name
                                                 ,itype_t.char_t );
                        //newCons.add( constraint.InnerText );
                        processConstraint( constraint.InnerText, ref newCons );
                        break;
                    }
                    case "int":
                    case "Int":
                    case "INT":
                    case "integer":
                    case "Integer": {
                        newCons = new Constraint( constraint.Name
                                                 ,itype_t.integer_t );
                        //int i = 0;
                        //if ( int.TryParse( constraint.InnerText, out i ) ) {
                        //    newCons.add( i );
                        //}
                        processConstraint( constraint.InnerText, ref newCons );
                        break;
                    }
                    case "float":
                    case "Float":
                    case "FLOAT": //{
                    //    newCons = new Constraint( constraint.Name
                    //                             ,itype_t.float_t );
                    //    double i = 0.0;
                    //    if ( double.TryParse( constraint.InnerText, out i ) ) {
                    //        newCons.add( i );
                    //    }
                    //    break;
                    //}
                    case "double":
                    case "decimal":
                    case "Decimal":
                    case "Double":
                    case "DOUBLE": {
                        newCons = new Constraint( constraint.Name
                                                 ,itype_t.double_t );
                        //double i = 0.0;
                        //if ( double.TryParse( constraint.InnerText, out i ) ) {
                        //    newCons.add( i );
                        //}
                        processConstraint( constraint.InnerText, ref newCons );
                        break;
                    }
                    case "str":
                    case "string":
                    case "String":
                    case "STRING": {
                        newCons = new Constraint( constraint.Name
                                                 ,itype_t.string_t );
                        //newCons.add( constraint.InnerText );
                        processConstraint( constraint.InnerText, ref newCons );
                        break;
                    }
                    default:
                    {
                        newCons = new Constraint();
                        processConstraint( constraint.InnerText, ref newCons );
                        break;
                    }// end default
                }// end switch
                Constraints.Add( constraint.Name, newCons );
            }// end foreach
        }// end processConstraints

        private void processConstraint( string strval, ref Constraint newCons )
        {

            if ( ( strval.StartsWith( "(" ) ) &&
                 ( strval.EndsWith( ")" ) ) )
            {   // break out comma delineated possible values
                // determine if integer or string and add to list
                if ( strval.IndexOf( ")" ) -
                     strval.IndexOf( "(" ) <= 1 )
                {   // empty braces ()
                    return;
                }
                if ( !strval.Contains( ',' ) )
                {
                    string strVal = strval.Substring( 1, strval.Length );
                    processSingleVal( strVal, ref newCons );
                }
                string[] values = (
                    strval.Substring( 1, ( strval.Length - 2 ) ) ).Split( ',' );
                try
                {
                    int i = 0;
                    if ( ( int.Parse( values[ 0 ] ) ) >= int.MinValue )
                    {
                        newCons.typeVal = itype_t.integer_t;
                        foreach ( string str in values )
                        {
                            if ( int.TryParse( str, out i ) )
                            {
                                if ( newCons.upperBound < i )
                                {
                                    newCons.upperBound = i;
                                }
                                if ( newCons.lowerBound > i )
                                {
                                    newCons.lowerBound = i;
                                }
                                newCons.add( i );
                            }
                        }// end foreach
                    }
                }
                catch ( FormatException ex )
                {
                    double i = 0;
                    if ( double.TryParse( values[ 0 ], out i ) )
                    {
                        newCons.typeVal = itype_t.double_t;
                        foreach ( string str in values )
                        {
                            if ( double.TryParse( str, out i ) )
                            {
                                if ( newCons.upperDBound < i )
                                {
                                    newCons.upperDBound = i;
                                }
                                if ( newCons.lowerDBound > i )
                                {
                                    newCons.lowerDBound = i;
                                }
                                newCons.add( i );
                            }
                        }// end foreach
                    }
                    else
                    { // expect string
                        newCons.typeVal = itype_t.string_t;
                        bool charSeriesTest = true;
                        foreach ( string str in values )
                        {
                            newCons.add( str );
                            if ( str.Length > 1 )
                            {
                                charSeriesTest = false;
                            }
                        }// end foreach
                        if ( charSeriesTest )
                        {
                            newCons.typeVal = itype_t.char_t;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( "EXCEPTION: " + ex.Data +
                                                        Environment.NewLine );
                }
            }
            else
            { // treat like some single value but try to guess type
                processSingleVal( strval, ref newCons );
            }
        }// end processConstraint

        private void processSingleVal( string strval, ref Constraint newCons )
        {
            try {
                int i = 0;
                if ( ( int.Parse( strval ) ) >= int.MinValue ) {
                    newCons.typeVal = itype_t.integer_t;
                    newCons.upperBound = i;
                    newCons.add(i);
                }
            }
            catch ( FormatException ex )
            {
                double i = 0;
                if ( double.TryParse( strval, out i ) ) {
                    newCons.typeVal = itype_t.double_t;
                    newCons.upperDBound = i;
                    newCons.add( i );
                }
                else if ( strval.Length == 1 ) {
                    newCons.typeVal = itype_t.char_t;
                    newCons.add( strval );
                }
                else { // expect string
                    newCons.typeVal = itype_t.string_t;
                    newCons.add( strval );
                }
            }
        }

        public void processSimulationObjects()
        {
            XmlNode simRoot = root.SelectSingleNode( SIMULATIONTAG );
            foreach ( XmlNode rootChild in simRoot ) {
                ProcessAllTheThings( rootChild );
            }
        }

        public void ProcessAllTheThings( XmlNode node )
        {   // check for verifier list interest
            try {
                //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( "node: " + node.Name + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( " ## CheckDeps ## " + Environment.NewLine );
                CheckDeps( ref node );
                //System.Diagnostics.Debug.WriteLine( " ## CheckSubTypes ## " + Environment.NewLine );
                CheckSubTypes( ref node );
                //System.Diagnostics.Debug.WriteLine( " ## CheckMetaTypes ## " + Environment.NewLine );
                CheckMetaTypes( ref node );
                //System.Diagnostics.Debug.WriteLine( " ## CheckRefs ## " + Environment.NewLine );
                CheckRefs( ref node );
                //System.Diagnostics.Debug.WriteLine( " ## CheckFieldRefs ## " + Environment.NewLine );
                CheckFieldRefs( ref node );
                //System.Diagnostics.Debug.WriteLine( " ## CheckSubTypeRefs ## " + Environment.NewLine );
                CheckSubTypeRefs( ref node );
                //System.Diagnostics.Debug.WriteLine( " ## CheckMetaTypeRefs ## " + Environment.NewLine );
                CheckMetaTypeRefs( ref node );
                //System.Diagnostics.Debug.WriteLine( " ## CheckScopeTo ## " + Environment.NewLine );
                CheckScopeTo( ref node );
                // check for container list interest
                //if ( ! string.IsNullOrEmpty( node.container() ) ) {
                //    return;
                //}
                if ( ! node.HasChildNodes ) {
                    return;
                }
                //
                string key = node.ParentNode.Name + KEYSEPARATOR + node.Name;
                if ( ( node.ParentNode != null ) &&
                     ( ! RequestDescriptor.ContainsKey( key ) ) )
                {
                    RequestDescriptor.Add( key, new List<string>() );
                }
                foreach ( XmlNode child in node.ChildNodes )
                {
                    if ( ! RequestDescriptor[ key ].Contains( child.Name ) )
                    {
                        RequestDescriptor[ key ].Add( child.Name );
                    }
                    ProcessAllTheThings( child );
                } // end if foreach
                //
            }
            catch ( Exception ex ) {
                System.Diagnostics.Debug.WriteLine( "ERROR: " + ex.ToString() + Environment.NewLine );
            }
        }// end findContainers
        //
        // Debug print all the lists
        //
        private void printAllTheThings()
        {
            System.Diagnostics.Debug.WriteLine( "printAllTheThings... " + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "RequestDescriptor... " + Environment.NewLine );
            //foreach ( KeyValuePair<string, List<string>> pair in RequestDescriptor )
            //{
            //    System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key + Environment.NewLine );
            //    foreach( string str in  pair.Value ) {
            //        System.Diagnostics.Debug.WriteLine( "Value: " + str + Environment.NewLine );
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "TypeAssociations... " + Environment.NewLine );
            //foreach ( KeyValuePair<string, List<string>> pair in TypeAssociations )
            //{
            //    System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key + Environment.NewLine );
            //    foreach ( string str in pair.Value )
            //    {
            //        System.Diagnostics.Debug.WriteLine( "Value: " + str + Environment.NewLine );
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "ReferenceAssocs... " + Environment.NewLine );
            //foreach ( KeyValuePair<string, List<string>> pair in ReferenceAssocs )
            //{
            //    System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key + Environment.NewLine );
            //    foreach ( string str in pair.Value )
            //    {
            //        System.Diagnostics.Debug.WriteLine( "Value: " + str + Environment.NewLine );
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "DependencyLists... " + Environment.NewLine );
            //foreach ( KeyValuePair<string, List<string>> pair in DependencyLists )
            //{
            //    System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key + Environment.NewLine );
            //    foreach ( string str in pair.Value ) {
            //        System.Diagnostics.Debug.WriteLine( "Value: " + str + Environment.NewLine );
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "Groups... " + Environment.NewLine );
            //foreach ( KeyValuePair<string, List<string>> pair in Groups )
            //{
            //    System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key + Environment.NewLine );
            //    foreach ( string str in pair.Value )
            //    {
            //        System.Diagnostics.Debug.WriteLine( "Value: " + str + Environment.NewLine );
            //    }
            //}
            System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            System.Diagnostics.Debug.WriteLine( "Constraints... " + Environment.NewLine );
            foreach ( KeyValuePair<string, Constraint> pair in Constraints ) {
                System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key + Environment.NewLine );
                System.Diagnostics.Debug.WriteLine( "Value: " + pair.Value.ToString() + Environment.NewLine );
            }
            System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "FieldRefs... " + Environment.NewLine );
            //foreach ( KeyValuePair<KeyValuePair<string,string>, List<string>> pair in FieldRefs )
            //{
            //    System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key.Key + KEYBINDER + pair.Key.Value + Environment.NewLine );
            //    foreach ( string str in pair.Value ) {
            //        System.Diagnostics.Debug.WriteLine( "Value: " + str + Environment.NewLine );
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "ScopeToList... " + Environment.NewLine );
            //foreach ( KeyValuePair<string, List<string>> pair in ScopeToList ) {
            //    System.Diagnostics.Debug.WriteLine( "Key: " + pair.Key + Environment.NewLine );
            //    foreach ( string str in pair.Value ) {
            //        System.Diagnostics.Debug.WriteLine( "Value: " + str + Environment.NewLine );
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine( "############################################################" + Environment.NewLine );
        }
        //
        //  ProcessAllTheThings SubRoutines
        //
        // Check for dependency target's list and add this node to the 
        // dependency target's list
        private void CheckDeps( ref XmlNode node )
        {   // dependencies
            if ( string.IsNullOrEmpty( node.depends() ) ) {
                return;
            }
            if ( ! DependencyLists.ContainsKey( node.depends() ) ) {
                DependencyLists.Add( node.depends()
                                    ,new List<string>());
            }
            if ( ! DependencyLists[ node.depends() ].Contains( node.Name ) ) {
                DependencyLists[ node.depends() ].Add( node.Name );
            }
        }
        // check for type's list and add this type to that type's possible
        // values list
        private void CheckSubTypes( ref XmlNode node )
        {
            if ( string.IsNullOrEmpty( node.subType() ) ) {
                return;
            }
            List<string> tmp;
            if ( node.subType().Contains(',') ) {
                tmp = node.subType().Split( ',' ).ToList();
                tmp.RemoveAll( item => item == String.Empty );
                if ( tmp.Count <= 0 ) {
                    return;
                }
            }
            else {
                tmp = new List<string>();
                tmp.Add( node.subType() );
            }
            foreach( string str in tmp ) {
                if ( ! TypeAssociations.ContainsKey( str ) ) {
                    TypeAssociations.Add( str, new List<string>() );
                }
                if ( ! TypeAssociations[ str ].Contains( node.Name ) ) {
                    TypeAssociations[ str ].Add( node.Name );
                }
            }//end foreach
        }
        // check for logical grouping and add this node to that logical grouping's list 
        private void CheckMetaTypes( ref XmlNode node )
        {
            if ( string.IsNullOrEmpty( node.metaType() ) ) {
                return;
            }
            if ( ! Groups.ContainsKey( node.metaType() ) ) {
                Groups.Add( node.metaType()
                           ,new List<string>() );
            }
            if ( ! Groups[ node.metaType() ].Contains( node.Name ) ) {
                Groups[ node.metaType() ].Add( node.Name );
            }
        }
        // checks for referenced type list, adds this node name to that list to 
        // act as filter of possible types for that reference type
        private void CheckRefs( ref XmlNode node )
        {
            if ( string.IsNullOrEmpty( node.refType() ) ) {
                return;
            }
            if ( ! ReferenceAssocs.ContainsKey( node.refType() ) ) {
                ReferenceAssocs.Add( node.refType()
                                    ,new List<string>() );
            }
            if ( ! ReferenceAssocs[ node.refType() ].Contains( node.Name ) ) {
                ReferenceAssocs[ node.refType() ].Add( node.Name );
            }
        }
        // if referenceField set, add to FieldRefs such that Target is key
        private void CheckFieldRefs( ref XmlNode node )
        {
            if ( string.IsNullOrEmpty( node.refFieldType() ) ) {
                return;
            }
            if ( string.IsNullOrEmpty( node.refType() ) ) {
                return;
            }
            KeyValuePair<string,string> tmp = 
                new KeyValuePair<string, string>( node.refFieldType()
                                                 ,node.refType() );
            if ( ! FieldRefs.ContainsKey( tmp ) ) {
                FieldRefs.Add( tmp, new List<string>() );
            }
            if ( ! FieldRefs[ tmp ].Contains( node.Name ) ) {
                FieldRefs[ tmp ].Add( node.Name );
            }
        }
        // check for type logical sub groupings
        private void CheckSubTypeRefs( ref XmlNode node )
        {
            if ( string.IsNullOrEmpty( node.refSubType() ) ) {
                return;
            }
            if ( ! TypeAssociations.ContainsKey( node.refSubType() ) ) {
                TypeAssociations.Add( node.refSubType()
                                     ,new List<string>() );
            }
            if ( ! TypeAssociations[ node.refSubType() ].Contains( node.Name ) ) {
                TypeAssociations[ node.refSubType() ].Add( node.Name );
            }
        }
        // check for logical groupings
        private void CheckMetaTypeRefs( ref XmlNode node )
        {
            if ( string.IsNullOrEmpty( node.refMetaType() ) ) {
                return;
            }
            if ( ! Groups.ContainsKey( node.refMetaType() ) ) {
                Groups.Add( node.refMetaType()
                           ,new List<string>() );
            }
            if ( ! Groups[ node.refMetaType() ].Contains( node.Name ) ) {
                Groups[ node.refMetaType() ].Add( node.Name );
            }
        }
        // if UniqueScopeTo set, capture syntax such that Target location is 
        // key and name is value for verification
        private void CheckScopeTo( ref XmlNode node )
        {
            if ( string.IsNullOrEmpty( node.scopeTo() ) ) {
                return;
            }
            if ( ! ScopeToList.ContainsKey( node.scopeTo() ) ) {
                ScopeToList.Add( node.scopeTo()
                                ,new List<string>() );
            }
            if ( ! ScopeToList[ node.scopeTo() ].Contains( node.Name ) ) {
                ScopeToList[ node.scopeTo() ].Add( node.Name );
            }
        }
        //
        public XmlNode FindObject( ref SearchDesc crit, XmlNode node )
        {   // check for verifier list interest
            if  ( crit.isMatch( ref node ) ) { // handle success case
                return node;
            }
            XmlNode retVal = null;
            foreach ( XmlNode child in node.ChildNodes ) {
                retVal = FindObject( ref crit, child );
                if ( retVal != null ) {
                    return retVal;
                }
            } // end if foreach
            return null;
        }
        //

        //
        //
        //// baysian element tester
        //// entry points 
        ////  context sensitive call
        ////  validator call
        ///*
        // * group
        // * type
        // * subType
        // * metaType
        // * constraint
        // * EngineDelin
        // * isUniqueScope -- UniqueScopeTo
        // */
        //public bool isValid()
        //{

        //}
        //
    }// end xdmToRequestDescriptor
    //
}
