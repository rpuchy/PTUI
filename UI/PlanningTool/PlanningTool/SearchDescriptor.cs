using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RequestRepresentation
{
    public class SearchDesc
    {
        private int         testCount;
        private string      _name;
        private string      _parent;
        private string      _type;
        private string      _subType;
        private string      _metaType;
        private string      _reference;
        private string      _referenceField;
        private string      _referenceSubType;
        private string      _referenceMetaType;
        private bool        _isUniqueScope;
        //private Constraint  _limit;
        //private List<string> _deps;
        //
        public SearchDesc()
        {
            testCount = 0;
        }
        //
        public SearchDesc( string name
                          ,string parent )
        {
            _name = name;
            _parent = parent;
            testCount = 2;
        }// end ctor
        //
        public SearchDesc( string name
                          ,string type
                          ,string subType
                          ,string metaType )
        {
            _name               = name;
            _type               = type;
            _subType            = subType;
            _metaType           = metaType;
            testCount           = 4;
        }// end ctor
        //
        public SearchDesc( string name
                          ,string type
                          ,string subType
                          ,string metaType
                          ,string reference
                          ,string referenceField
                          ,string referenceSubType
                          ,string referenceMetaType
                          ,bool   isUniqueScope )
        {
            _name               = name;
            _type               = type;
            _subType            = subType;
            _metaType           = metaType;
            _reference          = reference;
            _referenceField     = referenceField;
            _referenceSubType   = referenceSubType;
            _referenceMetaType  = referenceMetaType;
            _isUniqueScope      = isUniqueScope;
            testCount           = 9;
        }// end ctor
        //
        public int ArgC() {
            return testCount;
        }
        //
        public bool isMatch( ref XmlNode target )
        {
            int thisTest = 0;
            //
            //System.Diagnostics.Debug.WriteLine( " ################################### " + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "criteria search _Name: " + _name );
            //System.Diagnostics.Debug.WriteLine( "  target.Name: " + target.Name + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( "criteria search _parent: " + _parent );
            //System.Diagnostics.Debug.WriteLine( "  target.ParentNode.Name: " + target.ParentNode.Name + Environment.NewLine );
            //System.Diagnostics.Debug.WriteLine( " ################################### " + Environment.NewLine );
            //
            if ( ( ! string.IsNullOrEmpty( _parent ) )  &&
                 ( target.ParentNode != null )          &&
                 ( target.ParentNode.Name.Equals( _parent
                                                 ,StringComparison.InvariantCultureIgnoreCase ) ) )
            {
                thisTest += 1;
                //System.Diagnostics.Debug.WriteLine( "   Parent Match :" + thisTest + " of " + testCount + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( " ################################### " + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( "criteria search _Name: " + _name );
                //System.Diagnostics.Debug.WriteLine( "  target.Name: " + target.Name + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( "criteria search _parent: " + _parent );
                //System.Diagnostics.Debug.WriteLine( "  target.ParentNode.Name: " + target.ParentNode.Name + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( " ################################### " + Environment.NewLine );
            }
            if ( ( !string.IsNullOrEmpty( _name ) ) && 
                 ( target.Name != null )            &&
                 ( target.Name.Equals( _name
                                      ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
                //System.Diagnostics.Debug.WriteLine( "   Name Match :" + thisTest + " of " + testCount + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( " ################################### " + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( "criteria search _Name: " + _name );
                //System.Diagnostics.Debug.WriteLine( "  target.Name: " + target.Name + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( "criteria search _parent: " + _parent );
                //System.Diagnostics.Debug.WriteLine( "  target.ParentNode.Name: " + target.ParentNode.Name + Environment.NewLine );
                //System.Diagnostics.Debug.WriteLine( " ################################### " + Environment.NewLine );
            }
            if ( thisTest == testCount ) {
                //System.Diagnostics.Debug.WriteLine( "returning true, thisTest: " + thisTest + " testCount:" + testCount + Environment.NewLine );
                return true; // heavily used case, premature optimisation
            }
            if ( ( !string.IsNullOrEmpty( _type ) ) &&
                 ( target.TypeStr() != null )       &&
                 ( target.TypeStr().Equals( _type
                                           ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
            }
            if ( ( !string.IsNullOrEmpty( _subType ) )  && 
                 ( target.subType() != null )           &&
                 ( target.subType().Equals( _subType
                                           ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
            }
            if ( ( !string.IsNullOrEmpty( _metaType ) ) && 
                 ( target.metaType() != null )          &&
                 ( target.metaType().Equals( _metaType
                                            ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
            }
            if ( ( !string.IsNullOrEmpty( _reference ) )    && 
                 ( target.refType() != null )               &&
                 ( target.refType().Equals( _reference
                                           ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
            }
            if ( ( !string.IsNullOrEmpty( _referenceField ) ) && 
                 ( target.refFieldType() != null )            &&
                 ( target.refFieldType().Equals( _referenceField
                                                ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
            }
            if ( ( !string.IsNullOrEmpty( _referenceSubType ) ) && 
                 ( target.refSubType() != null )                &&
                 ( target.refSubType().Equals( _referenceSubType
                                              ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
            }
            if ( ( !string.IsNullOrEmpty( _referenceMetaType ) ) && 
                 ( target.refMetaType() != null )                &&
                 ( target.refMetaType().Equals( _referenceMetaType
                                               ,StringComparison.InvariantCultureIgnoreCase ) ) ) {
                thisTest += 1;
            }
            if ( ( _isUniqueScope != false ) &&
                 ( target.isUnique() == _isUniqueScope ) ) {
                //System.Diagnostics.Debug.WriteLine( "_isUniqueScope: " + _isUniqueScope + Environment.NewLine );
                thisTest += 1;
            }
            if ( thisTest == testCount ) {
                //System.Diagnostics.Debug.WriteLine( "thisTest: " + thisTest + " testCount: " + testCount + Environment.NewLine );
                return true;
            }
            return false;
        }
        //
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) )
                {
                    testCount -= 1;
                }
                else
                {
                    testCount += 1;
                }
                _name = value;
            } // end set
        }// end type
        //
        public string parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) )
                {
                    testCount -= 1;
                }
                else
                {
                    testCount += 1;
                }
                _parent = value;
            } // end set
        }// end type
        //
        public string type
        {
            get {
                return _type;
            }
            set {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _type = value;
            } // end set
        }// end type
        //
        public string subType
        {
            get {
                return _subType;
            }
            set {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _subType = value;
            } // end set
        }// end type
        //
        public string metaType
        {
            get {
                return _metaType;
            }
            set {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _metaType = value;
            } // end set
        }// end type
        //
        public string reference
        {
            get {
                return _reference;
            }
            set {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _reference = value;
            } // end set
        }// end type
        //
        public string referenceField
        {
            get {
                return _referenceField;
            }
            set {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _referenceField = value;
            } // end set
        }// end type
        //
        public string referenceSubType
        {
            get {
                return _referenceSubType;
            }
            set {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _referenceSubType = value;
            } // end set
        }// end type
        //
        public string referenceMetaType
        {
            get {
                return _referenceMetaType;
            }
            set {
                if ( ( string.IsNullOrEmpty( value ) ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _referenceMetaType = value;
            } // end set
        }// end type
        //
        public bool isUniqueScope
        {
            get {
                return _isUniqueScope;
            }
            set {
                if ( ( value == false ) &&
                     ( testCount != 0 ) ) {
                    testCount-=1;
                }
                else {
                    testCount+=1;
                }
                _isUniqueScope = value;
            } // end set
        }// end type
        //
        //public Constraint limit
        //{
        //    get {
        //        return _limit;
        //    }
        //    set {
        //        if ( ( value == null ) &&
        //             ( testCount != 0 ) ) {
        //            testCount-=1;
        //        }
        //        else {
        //            testCount+=1;
        //        }
        //        _limit = value;
        //    } // end set
        //}// end type
        ////
        //public List<string> deps
        //{
        //    get {
        //        return _deps;
        //    }
        //    set {
        //        if ( ( _deps == null )    ||
        //             ( _deps.Count == 0 ) &&
        //             ( testCount != 0 ) ) {
        //            testCount -= 1;
        //        }
        //        else {
        //            testCount+=1;
        //        }
        //        _deps = value;
        //    } // end set
        //}// end type
        //
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append( "testCount: " + testCount + Environment.NewLine );
            str.Append( "_name: " + _name + Environment.NewLine );
            str.Append( "_parent: " + _parent + Environment.NewLine );
            str.Append( "_type: " + _type + Environment.NewLine );
            str.Append( "_subType: " + _subType + Environment.NewLine );
            str.Append( "_metaType: " + _metaType + Environment.NewLine );
            str.Append( "_reference: " + _reference + Environment.NewLine );
            str.Append( "_referenceField: " + _referenceField + Environment.NewLine );
            str.Append( "_referenceSubType: " + _referenceSubType + Environment.NewLine );
            str.Append( "_referenceMetaType: " + _referenceMetaType + Environment.NewLine );
            str.Append( "_isUniqueScope: " + _isUniqueScope + Environment.NewLine );
            return str.ToString();
        }
        //
    }// end SearchDesc
    //
}//
