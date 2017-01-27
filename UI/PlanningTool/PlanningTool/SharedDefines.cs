/*
 * Property of Milliman
 * Written By David.Neo
 * Written on: 4/08/2016
 * Purpose: XmlNode helpers for querying Milliman proprietary xdm format of a calculation engine request object
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
//
namespace RequestRepresentation
{   //
    /*
     * Load an engine XML input template file (probably as a default setting/on startup - let's call it the .XDM) which will have XML attributes which describe the universe of all possible objects which a valid request can have.
	This should then be used as a template for graphical manipulation and validation of requests, similar to a schema (but note engine input structure is not suited to a schema currently - we have no ambition to change this structure in the short term, so a schema is not appropriate). This Input Template will need to be created and maintained.
	Document each input object structure where not already done (e.g. inhttps://millimandigital.atlassian.net/wiki/display/F/Calculation+Engine+Structure)

	//
	 type="" 				: Required.  The type of object for any given element.  Valid values are:  
							  "container" a container object holding other objects
							  "scalar" a scalar with a value which can be set by a user.  Limits on its possible values are determined by the constraint attribute.
							  "const" a static scalar with a fixed value which cannot be set by a user.  Scalars of this type should never their value changed, the scalar value should always be set to the value specified by the Default attribute.						  
	 constraint="" 			: Optional.  A set of constraints on the potential values for the object.  Can be a primitive type, or a reference to a set of valid values held within <ConstraintLists>.
	 depends=""				: Optional.  If set then defines the behaviour in relation to another node.  Where this is set, the node will only appear where the conditions set in depends are true.  I assume we describe the condition using XPATH syntax.
							  Example: depends="../InflationAdjusted='true'" will move to the parent of the current element, find the <InflationAdjusted> child element within this, and only show / allow this element to be added or displayed if InflationAdjusted is set to true.
							  If omitted then the tag will have no dependency behaviours.
	 subType="" 			: Optional.  An optional filter for types.  This is generally the type of a specific Object, e.g. the type of Model
	 metaType="" 			: Optional.  An optional filter for types.  This is generally a grouping of like Objects which are to be filtered, e.g. the all interest rate Models will have metatype="InterestRate".
	 reference="" 		  	: Optional.  Used to indicate the tag types that a particular element can reference.  Acts as a filter for the set of possible values for the element.  Example: 'Model' might refer to a reference to the set of all Models.  The value to this element is then further defined by the referenceField attribute.  
							  If omitted or reference="" then scalar is assumed to not be a reference, with values determined solely by the constraint tag.
	 referenceField=""		: Optional. The field to be used in the filtered set of objects which is then used as the potential value for this scalar.  The value of this attribute references the child element of the filtered object set, whose values form the set of possible inputs for the element.  Example: referenceField="Name" means use the scalar values of the <Name> child element of the filtered set of objects as the set of possible values for this element.
	 referenceSubType=""	: Optional.  A further filter on the particular element SubTypes that this element can reference.  Acts as a filter for the set of possible values for the element. Example: referenceSubType='EQUITY' might reference all Models which are of sub-type EQUITY
	 referenceMetaType=""	: Optional.  A further filter on the particular element MetaTypes that this element can reference.  Acts as a filter for the set of possible values for the element. Example: referenceMetaType='InterestRate' might reference all Models which are of meta-type InterestRate
	 minOccurs="" 			: Optional.  The minimum number of occurrences of this element, and all of its child elements.  
							  If omitted assumed to be minOccurs="0".
	 maxOccurs="" 			: Optional.  The maximum number of occurrences of this element, and all of its child elements.  
							  If omitted assumed to be minOccurs="" (i.e. no limit on maximum elements).
	 default=""				: Optional. The default value for a scalar or const type (if defaulttype="scalar"), the name of a tag to be used as the default value (if defaulttype="reference") or a specified value for "date" references.  If ommitted then assume no default.
							  possible 
	 defaulttype=""			: Optional.  a flag to indicate the way in which the default value should be treated.  
							  If omitted then assume defaulttype="scalar".  Possible values are:
							  "scalar" (or omitted) indicates the default is a fixed value as per the schema.
							  "reference" indicates a reference to the value of a named element.  If this is set, the value of the default attribute is the name of the element referenced.  Note that here the suggested element should be found by moving up the tree and searching for the reference in succesive parent elements.  This may need to be amended in future if this logic is insufficient.
							  "date" indicates that the default value is a predefined set of dates.  Main use is to set default="today".
	 isUniqueScope=""		: Optional.  Boolean flag to determine whether the set of possible reference objects is limited to a particular uniquely defined scope/subset.  The scope/subset is determined by the UniqueScopeTo attribute. 
							  Default to "false" if omitted.  Valid values are:
							  "false" no restriction on the potential set of referenced objects
							  "true" the set of referenced objects is restricted to those objects which sit within the scope defined by UniqueScopeTo.  
	 UniqueScopeTo=""		: Optional.  Defines the set to which the scope of the possible reference objects is limited.  
							  Note that this is a reference to an element which is reached using XPATH syntax.  
							  This unique scope element will itself be referenced, and the nature of this set is determined by the values of reference in that element.
							  Example:  If UniqueScopeTo="../../../ToTaxWrapper", this element can take referenced values which are determined from the set of objects contained within the "FromTaxWrapper" element two levels above the current element in the hierarchy.  This might then be (say) the set of Product names in the referenced FromTaxWrapper.
	 desc=""				: Optional.  Free text description of the element.  Used for user interface tooltips etc.
							  If omitted then assumed blank.
	//
	<Any></Any>				: This indicates a free-form blank element which can be freely added and named to an object.  These can be readily added to any engine request and can be used as filters (e.g. in Queries), otherwise are completely ignored by the engine.
     */
    //
    //class KeyComparer : IEqualityComparer<XmlNode>
    //{
    //    public bool Equals( Key x, Key y )
    //    {
    //        return x.Name == y.Name;
    //    }

    //    public int GetHashCode( Key obj )
    //    {
    //        return obj.Name.GetHashCode();
    //    }
    //}
    //
    public class MutableKeyVal<Key, Val>
    {
        public Key key { get; set; }
        public Val val { get; set; }
        //
        public MutableKeyVal()
        {
        }
        //
        public MutableKeyVal( Key _key, Val _val )
        {
            this.key = _key;
            this.val = _val;
        }
    }
    //
    public class SharedBindings
    {
        public Dictionary<int, NodeInterfaceObject> _TreeXmlNodeBindings;
        public Dictionary<int, XmlNode> _KeyNodeBindings;
        public Dictionary<string, MutableKeyVal<int, int>> _NameToIdAndCollisionCounter;
        public AutoCompleteStringCollection _txtXpathAutoCompCol;
        public SharedBindings( ref Dictionary<int, XmlNode> KeyNodeBindings
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
        }// end ctor
    }
    //
    public class globalDefines
    {   // AttributeList
        public const string TYPE                = "type";
        public const string CONSTRAINT          = "constraint";
        public const string DEPENDS             = "depends";
        public const string SUBTYPE             = "subType";
        public const string METATYPE            = "metaType";
        public const string REFERENCE           = "reference";
        public const string REFERENCEFIELD      = "referenceField";
        public const string REFERENCESUBTYPE    = "referenceSubType";
        public const string REFERENCEMETATYPE   = "referenceMetaType";
        public const string MINOCCURS           = "minOccurs";
        public const string MAXOCCURS           = "maxOccurs";
        public const string DEFAULT             = "default";
        public const string DEFAULTTYPE         = "defaultType";
        public const string ISUNIQUESCOPE       = "isUniqueScope";
        public const string UNIQUESCOPETO       = "UniqueScopeTo";
        public const string DESC                = "desc";
        public const string CONTAINER           = "container";
        public const string KEYSEPARATOR        = "::";
    }// end globalDefines
    //
    public static class objectExtn
    {
        public static bool isNull( this object testTarget )
        {
            if ( testTarget == null )
            {
                return true;
            }
            return false;
        }//end is null
    }
    //
    public static class XmlNodeCustExtn
    {
        public static string TypeStr( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.TYPE ] != null ) ) {
                return me.Attributes[ globalDefines.TYPE ].Value;
            }
            return null;
        }
        public static string container( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.CONTAINER ] != null ) ) {
                return me.Attributes[ globalDefines.CONTAINER ].Value;
            }
            return null;
        }
        public static string constraint( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.CONSTRAINT ] != null ) ) {
                return me.Attributes[ globalDefines.CONSTRAINT ].Value;
            }
            return null;
        }
        public static string depends( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.DEPENDS ] != null ) ) {
                return me.Attributes[ globalDefines.DEPENDS ].Value;
            }
            return null;
        }
        public static string subType( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.SUBTYPE ] != null ) ) {
                return me.Attributes[ globalDefines.SUBTYPE ].Value;
            }
            return null;
        }
        public static string metaType( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.METATYPE ] != null ) ) {
                return me.Attributes[ globalDefines.METATYPE ].Value;
            }
            return null;
        }
        public static string refType( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.REFERENCE ] != null ) ) {
                return me.Attributes[ globalDefines.REFERENCE ].Value;
            }
            return null;
        }
        public static string refFieldType( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.REFERENCEFIELD ] != null ) ) {
                return me.Attributes[ globalDefines.REFERENCEFIELD ].Value;
            }
            return null;
        }
        public static string refSubType( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.REFERENCESUBTYPE ] != null ) ) {
                return me.Attributes[ globalDefines.REFERENCESUBTYPE ].Value;
            }
            return null;
        }
        public static string refMetaType( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.REFERENCEMETATYPE ] != null ) ) {
                return me.Attributes[ globalDefines.REFERENCEMETATYPE ].Value;
            }
            return null;
        }
        public static int minOcc( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.MINOCCURS ] != null ) ) {
                return int.Parse( me.Attributes[ globalDefines.MINOCCURS ].Value );
            }
            throw new Exception( "No minOccurs attribute on " + me.Name );
        }
        public static int maxOcc( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.MAXOCCURS ] != null ) ) {
                return int.Parse( me.Attributes[ globalDefines.MAXOCCURS ].Value );
            }
            throw new Exception( "No maxOccurs attribute on " + me.Name );
        }
        public static string defaultStr( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.DEFAULT ] != null ) ) {
                return me.Attributes[ globalDefines.DEFAULT ].Value;
            }
            return null;
        }
        public static string defaultType( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.DEFAULTTYPE ] != null ) ) {
                return me.Attributes[ globalDefines.DEFAULTTYPE ].Value;
            }
            return null;
        }
        public static bool isUnique( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.ISUNIQUESCOPE ] != null ) ) {
                try {
                    return bool.Parse( me.Attributes[ globalDefines.ISUNIQUESCOPE ].Value );
                }
                catch ( System.Exception ex ) {
                    System.Diagnostics.Debug.WriteLine( "EXCEPTION: " + ex.Data +
                                                        Environment.NewLine );
                    return false;
                }
            }
            return false;
        }
        public static string scopeTo( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.UNIQUESCOPETO ] != null ) ) {
                return me.Attributes[ globalDefines.UNIQUESCOPETO ].Value;
            }
            return null;
        }
        public static string desc( this XmlNode me ) {
            if ( ( me.Attributes != null ) &&
                 ( me.Attributes[ globalDefines.DESC ] != null ) ) {
                return me.Attributes[ globalDefines.DESC ].Value;
            }
            return null;
        }
        //
    }// end XmlNodeCustExtn
    //
    public static class XmlElementCustExtn
    {
        // Rename element
        // A somewhat expensive solution with a crate new node, append 
        // children to new node delete old node
        // ### NOTE Borrowed from MSDN example article on how to achieve rename
        //public static XmlElement RenameElement( XmlElement e, string newName )
        //{
        //    XmlDocument doc = e.OwnerDocument;
        //    XmlElement newElement = doc.CreateElement( newName );
        //    while ( e.HasChildNodes )
        //    {
        //        newElement.AppendChild( e.FirstChild );
        //    }
        //    XmlAttributeCollection ac = e.Attributes;
        //    while ( ac.Count > 0 )
        //    {
        //        newElement.Attributes.Append( ac[ 0 ] );
        //    }
        //    XmlNode parent = e.ParentNode;
        //    parent.ReplaceChild( newElement, e );
        //    return newElement;
        //}
        public static void AddWithNewName( XmlElement target
                                          ,XmlElement elemRN
                                          ,string newName )
        {
            if ( ( target == null ) ||
                 ( elemRN == null ) ||
                 ( target.OwnerDocument == null ) )
            {
                System.Diagnostics.Debug.WriteLine( "RenameElement: elem or " +
                                                    "OwnerDocument null" +
                                                    Environment.NewLine );
                return;
            }
            XmlDocument doc = target.OwnerDocument;
            XmlElement newElement = doc.CreateElement( newName );
            if ( elemRN.Attributes != null ) {
                XmlAttributeCollection ac = elemRN.Attributes;
                for ( int i = 0; i < ac.Count; ++i )
                {
                    XmlAttribute attr0 = doc.CreateAttribute( ac[i].Name );
                    attr0.Value = ac[ i ].Value;
                    newElement.Attributes.Append( attr0 );
                }
            }
            while ( elemRN.HasChildNodes ) {
                newElement.AppendChild( elemRN.FirstChild );
            }
            target.AppendChild( newElement );
        } // end AddWithNewName

        // Rename element
        // A somewhat expensive solution with a crate new node, append 
        // children to new node delete old node
        public static XmlElement RenameElement( XmlElement elem
                                               ,string newName
                                               ,bool destructiveAdd = true )
        {
            if ( ( elem == null ) ||
                 ( elem.OwnerDocument == null ) ) {
                System.Diagnostics.Debug.WriteLine( "RenameElement: elem or " +
                                                    "OwnerDocument null" +
                                                    Environment.NewLine );
                return null;
            }
            XmlDocument doc = elem.OwnerDocument;
            XmlElement newElement = doc.CreateElement( newName );
            if ( elem.Attributes != null ) {
                XmlAttributeCollection ac = elem.Attributes;
                for ( int i = 0; i < ac.Count; ++i )
                {
                    XmlAttribute attr0 = doc.CreateAttribute( ac[i].Name );
                    attr0.Value = ac[ i ].Value;
                    newElement.Attributes.Append( attr0 );
                }
            }
            while ( elem.HasChildNodes ) {
                newElement.AppendChild( elem.FirstChild );
            }
            //
            if ( destructiveAdd ) {
                if ( elem.ParentNode != null ) {
                    elem.ParentNode.AppendChild( newElement );
                }
                else { // no parent, either root or bad node
                    XmlNode currentNode = elem;
                    doc.AppendChild( currentNode );
                }
            }
            else {
                if ( elem.ParentNode != null ) {
                    XmlNode parent = elem.ParentNode;
                    parent.ReplaceChild( newElement, elem );
                }
                else { // no parent, either root or bad node
                    doc.ReplaceChild( newElement, elem );
                }
            }
            return newElement;
        }// end RenameElement
        //
    }// end XmlElementCustExtn
    //
    public static class stringConstraintExtn
    {
        public static bool isContainerString( this string str ) {
            if ( ( str != null ) &&
                 ( str.Equals( globalDefines.CONTAINER
                              ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                return true;
            }
            return false;
        }
        //
        public static DateTime Today() {
            return DateTime.Now;
        }
        //
        public static bool isValidDate( this string date )
        {
            DateTime tester;
            if ( DateTime.TryParse( date, out tester ) ) {
                return true;
            }
            return false;
        }
        //
        public static bool isValidInt( this string val )
        {
            int i = 0;
            if ( int.TryParse( val, out i ) ) {
                return true;
            }
            return false;
        }
        //
        public static bool isValidDouble( this string val )
        {
            double i = 0;
            if ( double.TryParse( val, out i ) ) {
                return true;
            }
            return false;
        }
        //
        public static bool isChar( this string str )
        {
            if ( str.Length == 1 ) {
                return true;
            }
            return false;
        }
    }//end stringConstraintExtn
    //
}// end namespace
