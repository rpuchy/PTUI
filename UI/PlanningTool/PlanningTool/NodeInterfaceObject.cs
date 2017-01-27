using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;


namespace RequestRepresentation
{
    //
    public class NodeInterfaceObject
    {
        private const String IDKEY = "ID";
        private const String PARENTIDKEY = "ParentID";
        private const string SIMULATIONTAG = "Simulation";
        //
        private static int NodeCount = 0;
        private static Dictionary<int, NodeInterfaceObject> _TreeXmlNodeBindings;
        private static Dictionary<int, XmlNode> _KeyNodeBindings;
        private static Dictionary<string, MutableKeyVal<int, int>> _NameToIdAndCollisionCounter;
        private static AutoCompleteStringCollection _txtXpathAutoCompCol;
        private String _nodename;
        private String _objecttype;
        private String _objectname;
        private String _constraint;
        private XmlNode _xmlnode;
        private String _value;
        private bool _haschildelements;
        private string _baseuri;
        private int _ID = -1;
        private int _ParentID = -1;
        //
        public NodeInterfaceObject()
        {
        }
        //
        // !BEWARE! this ctor is not declared const on your XmlNode for a reason
        public NodeInterfaceObject( XmlNode xmlnode )
        {
            initNodeInterfaceObject( xmlnode );
        }
        // !BEWARE! this ctor is not declared const on your XmlNode for a reason
        public NodeInterfaceObject( XmlNode xmlnode
                                   ,ref Dictionary<int, XmlNode> KeyNodeBindings
                                   ,ref Dictionary<int, NodeInterfaceObject> TreeXmlNodeBindings
                                   ,ref Dictionary<string, MutableKeyVal<int, int>> NameToIdAndCollisionCounter
                                   ,ref AutoCompleteStringCollection txtXpathAutoCompCol )
        {
            setStaticBindings( ref KeyNodeBindings
                              ,ref TreeXmlNodeBindings
                              ,ref NameToIdAndCollisionCounter
                              ,ref txtXpathAutoCompCol );
            initNodeInterfaceObject( xmlnode );
        }
        // !BEWARE! this ctor is not declared const on your XmlNode for a reason
        public NodeInterfaceObject( XmlNode xmlnode
                                   ,ref SharedBindings arg )
        {
            setStaticBindings( ref arg );
            initNodeInterfaceObject( xmlnode );
        }
        //
        public XmlNode getXmlNode() { return _xmlnode; }
        //
        public int ChildCount()
        {
            return _xmlnode.ChildNodes.Count;
        }
        //
        public void setStaticBindings( ref SharedBindings arg )
        {
            setStaticBindings( ref arg._KeyNodeBindings
                              ,ref arg._TreeXmlNodeBindings
                              ,ref arg._NameToIdAndCollisionCounter
                              ,ref arg._txtXpathAutoCompCol );
        }
        //
        public void setStaticBindings( ref Dictionary<int, XmlNode> KeyNodeBindings
                                      ,ref Dictionary<int, NodeInterfaceObject> TreeXmlNodeBindings
                                      ,ref Dictionary<string, MutableKeyVal<int, int>> NameToIdAndCollisionCounter
                                      ,ref AutoCompleteStringCollection txtXpathAutoCompCol )
        {
            if ( KeyNodeBindings != null ) {
                _KeyNodeBindings = KeyNodeBindings;
            }
            if ( TreeXmlNodeBindings != null ) {
                _TreeXmlNodeBindings = TreeXmlNodeBindings;
            }
            if ( NameToIdAndCollisionCounter != null ) {
                _NameToIdAndCollisionCounter = NameToIdAndCollisionCounter;
            }
            if ( txtXpathAutoCompCol != null ) {
                _txtXpathAutoCompCol = txtXpathAutoCompCol;
            }
        }
        //
        //public bool Equals( NodeInterfaceObject _comp )
        //{
        //    return ( _xmlnode.OuterXml.Equals( _comp.getXmlNode().OuterXml ) );
        //}
        //
        //public bool Equals( NodeInterfaceObject _comp )
        //{
        //    return ( ( _xmlnode.OuterXml.Equals( _comp.getXmlNode().OuterXml ) ) &&
        //             ( String.Equals( _nodename, _comp._nodename ) ) &&
        //             ( String.Equals( _objecttype, _comp.NodeName ) ) &&
        //             ( String.Equals( _objectname, _comp.ObjectName ) ) &&
        //             ( String.Equals( _value, _comp.Value ) ) );
        //}
        //
        public override bool Equals( object _comp )
        {   // this null test guards against foibles in the unfinished tree 
            // renderer lib we are utilising
            if ( _comp == null ) {
                return false;
            }
            return ( ( String.Equals( this._nodename
                                     ,((NodeInterfaceObject)_comp)._nodename ) )  &&
                     ( String.Equals( this._objecttype
                                     ,((NodeInterfaceObject)_comp)._objecttype ) )&&
                     ( String.Equals( this._objectname
                                     ,((NodeInterfaceObject)_comp)._objectname ) ) );// &&
                     //( String.Equals( this._value
                     //                ,((NodeInterfaceObject)_comp).Value ) ) );
        }
        //
        //public static bool operator ==( NodeInterfaceObject left
        //                               ,NodeInterfaceObject right )
        //{
        //    if ( ( left == null ) || ( right == null ) ) {
        //        return false;
        //    }
        //    return left.Equals( right );
        //}
        ////
        //public static bool operator !=( NodeInterfaceObject left
        //                               ,NodeInterfaceObject right )
        //{
        //    if ( ( left == null ) && ( right == null ) ) {
        //        return true;
        //    }
        //    else if ( ( left == null ) || ( right == null ) ) {
        //        return false;
        //    }
        //    return ! left.Equals( right );
        //}
        //
        public override int GetHashCode()
        {
            return Tuple.Create( _nodename, _objecttype, _objectname ).GetHashCode();
            //return this._nodename.GetHashCode() ^
            //       this._objecttype.GetHashCode() ^
            //       this._objectname.GetHashCode() ^
            //       this._value.GetHashCode();
        }
        //
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append( "_nodename: " + this._nodename + Environment.NewLine );
            str.Append( "_objecttype: " + this._objecttype + Environment.NewLine );
            str.Append( "_constraint: " + this._constraint + Environment.NewLine );
            str.Append( "_objectname: " + this._objectname + Environment.NewLine );
            str.Append( "_value: " + this._value + Environment.NewLine );
            str.Append( "_baseuri: " + this._baseuri + Environment.NewLine );
            return str.ToString();
        }
        //
        public bool hasParent()
        {
            if ( _xmlnode.ParentNode != null ) {
                return true;
            }
            return false;
        }

        public bool hasParentContainer()
        {
            if ( ( _xmlnode.ParentNode != null ) &&
                 ( _xmlnode.ParentNode.container() != null ) )
            {
                return true;
            }
            return false;
        }

        public void appendToParent( ref NodeInterfaceObject node )
        {
            _xmlnode.ParentNode.AppendChild( node._xmlnode );
        }

        public void appendToParent( ref XmlNode node )
        {
            _xmlnode.ParentNode.AppendChild( node );
        }

        public void append( IList<NodeInterfaceObject> nodeList )
        {
            foreach ( NodeInterfaceObject node in nodeList ) {
                _xmlnode.AppendChild( node._xmlnode );
            }
        }

        public void append( NodeInterfaceObject node )
        {
            _xmlnode.AppendChild( node._xmlnode );
        }

        public void append( ref XmlNode node )
        {
            _xmlnode.AppendChild( node );
        }

        public void removeSelfFromParent()
        {
            _xmlnode.ParentNode.RemoveChild( _xmlnode );
        }

        public void removeSiblingFromParent( ref NodeInterfaceObject sibling )
        {
            _xmlnode.ParentNode.RemoveChild( sibling._xmlnode );
        }

        public void removeChild( ref NodeInterfaceObject child )
        {
            _xmlnode.RemoveChild( child._xmlnode );
        }

        public void insertBefore( ref XmlNode newNode, ref XmlNode targetNode )
        {
            _xmlnode.InsertBefore( newNode, targetNode );
        }

        private bool hasStamps( XmlNode _node )
        {
            if ( ( _node == null ) ||
                 ( _node.Attributes == null ) ||
                 ( _node.Attributes[ IDKEY ] == null ) ||
                 ( _node.Attributes[ PARENTIDKEY ] == null ) ||
                 ( string.IsNullOrEmpty( _node.Attributes[ IDKEY ].InnerXml ) ) ||
                 ( string.IsNullOrEmpty( _node.Attributes[ PARENTIDKEY ].InnerXml ) ) )
            {
                return false;
            }
            return true;
        }

        private void getStamps( XmlNode _node )
        {
            if ( ( _node == null ) ||
                 ( string.IsNullOrEmpty( _node.OuterXml ) ) ) {
                System.Diagnostics.Debug.WriteLine( "getStamps return due to null _xmlnode field" + 
                                                    Environment.NewLine );
                return;
            }
            switch ( _node.NodeType )
            {
                case XmlNodeType.Element: {
                    break;
                }
                default: {
                    return;
                }
            } // end switch nodetype
            if ( ( _node.Attributes != null ) ||
                 ( _node.Attributes[ IDKEY ] != null ) )
            {
                int.TryParse( _node.Attributes[ IDKEY ].InnerXml
                             ,out _ID );
            }
            if ( _node.Attributes[ PARENTIDKEY ] != null )
            {
                int.TryParse( _node.Attributes[ PARENTIDKEY ].InnerXml
                             ,out _ParentID );
            }
        }

        private void makeStampIds()
        {
            stampNodeIds( ref _xmlnode );
        }

        private void stampNodeIds( ref XmlNode node )
        {
            if ( null == node ) {
                System.Diagnostics.Debug.WriteLine( "stampNodeIds return due to null param" + 
                                                    Environment.NewLine );
                return;
            }
            switch ( node.NodeType )
            {
                case XmlNodeType.Element: {
                    break;
                }
                default: {
                    return;
                }
            } // end switch nodetype
            // is a valid noed type
            if ( node.Attributes == null ) {
                throw new System.Exception( node.Name + " has a Null attribute collection" );
            }
            if ( node.Attributes[ IDKEY ] == null )
            {
                XmlDocument tmpDoc = new XmlDocument();
                NodeCount += 1;
                XmlAttribute attr0 = tmpDoc.CreateAttribute( IDKEY );
                attr0.Value = NodeCount.ToString();
                _ID = NodeCount;
                node.Attributes.SetNamedItem( attr0 );
                //System.Diagnostics.Debug.WriteLine( "   ###     NodeID: " + NodeCount + " Name: " + node.Name + Environment.NewLine );
            }
            if ( node.Attributes[ PARENTIDKEY ] == null )
            {
                if ( ( node.ParentNode == null ) ||
                     ( node.Name == SIMULATIONTAG ) )
                {   // set parent id to self
                    XmlAttribute attr0 = node.OwnerDocument.CreateAttribute( PARENTIDKEY );
                    attr0.Value = _ID.ToString();
                    _ParentID = _ID;
                    node.Attributes.SetNamedItem( attr0 );
                }
                else if ( ( node.ParentNode.Attributes != null ) &&
                     ( node.ParentNode.Attributes[ IDKEY ] != null ) )
                {
                    XmlAttribute attr0 = node.OwnerDocument.CreateAttribute( PARENTIDKEY );
                    attr0.Value = node.ParentNode.Attributes[ IDKEY ].InnerXml;
                    int.TryParse( attr0.Value, out _ParentID );
                    node.Attributes.SetNamedItem( attr0 );
                }
            }// end ParentID or ParentNode
        }

        public void initNodeInterfaceObject( XmlNode xmlnode )
        {
            if ( xmlnode == null ) {
                throw new System.Exception( "NodeInterfaceObject cant build " + 
                                            "from a null argument" );
            }
            //
            // handle id stamping
            if ( ! hasStamps( xmlnode ) ) {
                stampNodeIds( ref xmlnode );
                //System.Diagnostics.Debug.WriteLine( "   ###     !hasStamps: " + xmlnode.Name + Environment.NewLine );
            }
            else {
                getStamps( xmlnode );
                //System.Diagnostics.Debug.WriteLine( "   ###     hasStamps: " + xmlnode.Name + Environment.NewLine );
            }
            // Handle naming
            //System.Diagnostics.Debug.WriteLine( "   ###     OLD NAME: " + xmlnode.Name + Environment.NewLine );
            if ( _NameToIdAndCollisionCounter != null )
            {
                if ( _NameToIdAndCollisionCounter.ContainsKey( xmlnode.Name ) )
                {   // Note Element test in stamp functions
                    // should not mutate on ctor
                    // should add to cache or ignore?
                    
                    
                    //if ( xmlnode.NodeType != XmlNodeType.Element ) {
                    //    throw new System.Exception( "Non Element NodeType detected " +
                    //                                "after ID stamping not expected" );
                    //}
                    //XmlElement tmpCast;
                    //tmpCast = xmlnode as XmlElement;
                    //String oldKey = xmlnode.Name;
                    //xmlnode = XmlElementCustExtn.RenameElement( tmpCast
                    //                                           ,xmlnode.Name +
                    //                                            ( _NameToIdAndCollisionCounter[ xmlnode.Name ] ).val );
                    //System.Diagnostics.Debug.WriteLine( "   ###     NEW NAME: " + xmlnode.Name + Environment.NewLine );
                    //( _NameToIdAndCollisionCounter[ oldKey ] ).val += 1; // update old key
                    ////
                    ////#REVIEW Yes I really wrote a class to not have to construct an obj to modify a value per.
                    ////new KeyValuePair<int, int>( _ID, ( _NameToIdAndCollisionCounter[ _nodename ] ).Value + 1 );
                    //_NameToIdAndCollisionCounter.Add( xmlnode.Name, new MutableKeyVal<int, int>( _ID, 0 ) );
                    //if ( _txtXpathAutoCompCol != null ) {
                    //    _txtXpathAutoCompCol.Add( xmlnode.Name );
                    //}
                }
                else {
                    _NameToIdAndCollisionCounter.Add( xmlnode.Name, new MutableKeyVal<int, int>( _ID, 0 ) );
                    if ( ( _txtXpathAutoCompCol != null ) &&
                         ( !_txtXpathAutoCompCol.Contains( xmlnode.Name ) ) )
                    {
                        _txtXpathAutoCompCol.Add( xmlnode.Name );
                    }
                }// end else
            }// end if _NameToIdAndCollisionCounter
            //
            //System.Diagnostics.Debug.WriteLine( "   ###     POST NAME: " + xmlnode.Name + Environment.NewLine );
            // add to relevant caches
            if ( ( _KeyNodeBindings != null ) &&
                 ( !_KeyNodeBindings.ContainsKey( _ID ) ) )
            {
                _KeyNodeBindings.Add( _ID, xmlnode );
            }
            if ( ( _TreeXmlNodeBindings != null ) &&
                 ( !_TreeXmlNodeBindings.ContainsKey( _ID ) ) )
            {
                _TreeXmlNodeBindings.Add( _ID, this );
            }
            // assign fields
            _xmlnode = xmlnode;
            _nodename = xmlnode.Name;
            //
            if ( xmlnode.SelectSingleNode("./Name") != null ) {
                _objectname = xmlnode.SelectSingleNode("./Name").InnerText;
            }
            else if ( xmlnode.Attributes != null )
            {
                if ( xmlnode.Attributes["Name"] != null ) {
                    _objectname = xmlnode.Attributes["Name"].InnerXml;
                }
                else if ( xmlnode.SelectSingleNode( "name" ) != null ) {
                    _objectname = xmlnode.SelectSingleNode( "name" ).InnerText;
                }
                else if ( xmlnode.SelectSingleNode("./QueryID") != null ) {
                    _objectname = xmlnode.SelectSingleNode("./QueryID").InnerText;
                }
                else if ( _nodename != null ) {
                    _objectname = _nodename;
                }
            }
            else if ( xmlnode.SelectSingleNode("./OperatorID") != null ) {
                _objectname = xmlnode.SelectSingleNode("./OperatorID").InnerText;
            }
            else if ( xmlnode.SelectSingleNode("./SetNumber") != null ) {
                _objectname = xmlnode.SelectSingleNode( "./SetNumber" ).InnerText;
            }
            else if ( _nodename != null ) {
                _objectname = _nodename;
            }
            //
            if ( xmlnode.SelectSingleNode("./Type") != null ) {
                _objecttype = xmlnode.SelectSingleNode("./Type").InnerText;
            }
            else if ( xmlnode.Attributes != null )
            {
                if ( xmlnode.Attributes["Type"] != null ) {
                    _objecttype = xmlnode.Attributes[ "Type" ].InnerXml;
                }
                else if ( xmlnode.Attributes[ "type" ] != null ) {
                    _objecttype = xmlnode.Attributes[ "type" ].InnerXml;
                }
                else if (xmlnode.Attributes["DataType"] != null) {
                    _objecttype = xmlnode.Attributes["DataType"].InnerXml;
                }
                else if ( xmlnode.Attributes[ "referenceType" ] != null ) {
                    _objecttype = xmlnode.Attributes[ "referenceType" ].InnerXml;
                }
            }
            else if ( xmlnode.SelectSingleNode("./Operation/Type") != null ) {
                _objecttype = xmlnode.SelectSingleNode("./Operation/Type").InnerText;
            }
            if ( xmlnode.Attributes != null ) {
                if ( xmlnode.Attributes[ "constraint" ] != null ) {
                    _constraint = xmlnode.Attributes[ "constraint" ].InnerXml;
                }
            }
            //
            _haschildelements = false;
            if ( xmlnode.HasChildNodes ) {
                foreach (XmlNode node in xmlnode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element) {
                        _haschildelements = true;
                        break;
                    }
                }// end foreach
            }
            if (!_haschildelements) {
                if ( ( xmlnode.Attributes != null )                     &&
                     ( xmlnode.Attributes[ "referenceType" ] != null )  &&
                     ( xmlnode.Attributes[ "referenceSubType" ] != null ) )
                {
                    _value = xmlnode.Attributes[ "referenceSubType" ].InnerXml;
                }
                else {
                    _value = xmlnode.InnerText;
                }
            }
            //
            _baseuri = xmlnode.BaseURI;
            //
        }// end ctor

        //
        //
        //  Method accessors
        //
        //

        public int ID
        {
            get { return _ID; }
        }

        public int ParentID
        {
            get { return _ParentID; }
        }

        public String NodeName
        {
            //get { if (_haschildelements) return _nodename; else return _nodename; }// +" (" + _value + ")"; }
            get { return _nodename; }
            //set
            //{
            //    _objecttype = value;
            //    _xmlnode.InnerXml = _xmlnode.InnerXml.Replace( _xmlnode.Name + ">", value + ">" );
            //}
        }

        public String ObjectName
        {
            get { return _objectname; }
            //set
            //{ 
            //    _objectname = value;
            //    if ( _xmlnode.SelectSingleNode( "./Name" ) != null )
            //    {
            //        _xmlnode.SelectSingleNode( "./Name" ).InnerText = value;
            //    }
            //    else if ( _xmlnode.Attributes != null )
            //    {
            //        if ( _xmlnode.Attributes[ "Name" ] != null )
            //        {
            //            _xmlnode.Attributes[ "Name" ].InnerText = value;
            //        }
            //        else if ( _xmlnode.SelectSingleNode( "name" ) != null )
            //        {
            //            _xmlnode.SelectSingleNode( "name" ).InnerText = value;
            //        }
            //        else if ( _xmlnode.SelectSingleNode( "./QueryID" ) != null )
            //        {
            //            _xmlnode.SelectSingleNode( "./QueryID" ).InnerText = value;
            //        }
            //    }
            //    else if ( _xmlnode.SelectSingleNode( "./OperatorID" ) != null )
            //    {
            //        _xmlnode.SelectSingleNode( "./OperatorID" ).InnerText = value;
            //    }
            //}// end set
        }
        public String ObjectTypeName
        {
            get { return _objecttype; }
            //set
            //{ 
            //    _objecttype = value;
            //    _xmlnode.Attributes[ "type" ].Value = value;
            //}
        }

        public String Value
        {
            get { return _value; }
            set 
            { 
                _value = value;
                _xmlnode.InnerText = value;
            }
        }

        public String Constraint
        {
            get { return _constraint; }
            set {
                _constraint = value;
            }
        }

        public bool CanExpand() {
            if ( null == _xmlnode ) {
                return false;
            }
            return _xmlnode.HasChildNodes;
        }

        public List<NodeInterfaceObject> GetChildren()
        {
            List<NodeInterfaceObject> children = new List<NodeInterfaceObject>();

            if ( ( _KeyNodeBindings == null ) ||
                 ( _TreeXmlNodeBindings == null ) )
            {   // no static caches, fall back to xmlnode
                foreach ( XmlNode child in _xmlnode.ChildNodes )
                {
                    children.Add( new NodeInterfaceObject( child ) );
                }
            }
            else 
            { // static caches with associated id's exist
                foreach (XmlNode child in _xmlnode.ChildNodes)
                {
                    if ( ( child.Attributes != null ) &&
                         ( child.Attributes[ IDKEY ] != null ) &&
                         ( string.IsNullOrEmpty( child.Attributes[ IDKEY ].InnerXml ) ) )
                    {
                        int tmp;
                        int.TryParse( child.Attributes[ IDKEY ].InnerXml, out tmp );
                        if ( _KeyNodeBindings.ContainsKey( tmp ) ) {
                            children.Add( _TreeXmlNodeBindings[ tmp ] );
                        }
                        else { // not in cache
                            children.Add( new NodeInterfaceObject( child ) );
                        }
                    }
                    else { // no id on node
                        children.Add( new NodeInterfaceObject( child ) );
                    }
                } // end foreach
            }// end else
            return children;
        }

        public bool IsAncestor( NodeInterfaceObject src )
        {
            NodeInterfaceObject me = this;
            return recursiveRelationCheck( me._xmlnode, src._xmlnode );
        }// end IsAncestor
        //
        private bool recursiveRelationCheck( XmlNode me, XmlNode targetNode )
        {
            if ( ( me.ParentNode == targetNode ) ||
                 ( me == targetNode ) )
            {
                return true;
            }
            foreach ( XmlNode child in targetNode.ChildNodes ) {
                if ( child == me ) {
                    return true;
                }
                foreach ( XmlNode grandChild in child.ChildNodes ) {
                    if ( recursiveRelationCheck( me, grandChild ) ) {
                        return true;
                    }
                }
            }// end foreach
            return false;
        }// end recursiveRelationCheck
        //
        public string getToolTip()
        {
            return this._xmlnode.desc();
        }
        //
    }//end NodeInterfaceObject
    //
}//end namespace
