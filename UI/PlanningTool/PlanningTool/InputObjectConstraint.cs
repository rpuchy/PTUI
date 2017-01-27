using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RequestRepresentation
{

    // Constraint class dependency for type determination and refferencing
    public enum itype_t
    {
        Unknown_t
        ,boolean_t
        ,integer_t
        ,decimal_t
        ,float_t
        ,double_t
        ,char_t
        ,string_t
        ,DateTime_t
        ,Uri_t
        ,Object_t
    }

    public class Constraint
    {
        public itype_t         typeVal      { get; set; }
        public string          type         { get; set; }
        public int             upperBound   { get; set; }
        public int             lowerBound   { get; set; }
        public double          upperDBound  { get; set; }
        public double          lowerDBound  { get; set; }
        public string          grouping     { get; set; }
        public List<double>    possibleDoubleValues { get; set; }
        public List<int>       possibleIntValues    { get; set; }
        public List<string>    possibleStrValues    { get; set; }
        public bool            isBool       { get; set; }
        //
        // This code makes me wish for c++ intialiser lists.
        //
        
        public Constraint() {
            ctorInit();
            typeVal = itype_t.Unknown_t;
        }
        //
        public Constraint( string typ, itype_t typVal ) {
            ctorInit();
            typeVal = typVal;
            type = typ;
        }
        //
        public Constraint( int upper, int lower )
        {
            ctorInit();
            typeVal = itype_t.integer_t;
            upperBound = upper;
            lowerBound = lower;
        }
        //
        public Constraint( double upper, double lower )
        {
            ctorInit();
            typeVal = itype_t.double_t;
            upperDBound = upper;
            lowerDBound = lower;
        }
        //
        public Constraint( string typ ) {
            ctorInit();
            typeVal = itype_t.Unknown_t;
            type = typ;
        }
        //
        public Constraint( ref string typ, ref string grp ) {
            ctorInit();
            typeVal = itype_t.Object_t;
            type = typ;
            grouping = grp;
        }
        //
        public Constraint( bool setToBool ) {
            ctorInit();
            typeVal = itype_t.boolean_t;
            isBool = true;
        }
        //
        private void ctorInit()
        {
            isBool = false;
            upperBound = 0;
            lowerBound = 0;
            upperDBound = 0.0;
            lowerDBound = 0.0;
            possibleDoubleValues = new List<double>();
            possibleIntValues = new List<int>();
            possibleStrValues = new List<string>();
        }
        //
        public int size()
        {
            switch ( typeVal )
            {
                case itype_t.string_t: {
                    return possibleStrValues.Count;
                }
                case itype_t.integer_t: {
                    return possibleIntValues.Count;
                }
                case itype_t.decimal_t:
                case itype_t.float_t:
                case itype_t.double_t: {
                    return possibleDoubleValues.Count;
                }
                case itype_t.Unknown_t:
                case itype_t.boolean_t:
                case itype_t.char_t:
                case itype_t.DateTime_t:
                case itype_t.Uri_t:
                case itype_t.Object_t:
                default: {
                    return -1;
                }
            }
        }
        //
        public void add( double val ) {
            possibleDoubleValues.Add( val );
        }
        //
        public void remove( double val ) {
            possibleDoubleValues.Remove( val );
        }
        //
        public void add( int val ) {
            possibleIntValues.Add( val );
        }
        public void remove( int val ) {
            possibleIntValues.Remove( val );
        }
        //
        public void add( string val ) {
            possibleStrValues.Add( val );
        }
        //
        public void remove( string val ) {
            possibleStrValues.Remove( val );
        }
        //
        public itype_t ConstraintType() {
            return typeVal;
        }
        //
        public bool isValid( int tester )
        {
            if ( tester != tester ) { // nan test
                return false;
            }
            if ( possibleIntValues.Count > 0 ) {
                if ( ! possibleIntValues.Contains( tester ) ) {
                    return false;
                }
                return true;
            }
            if ( ( upperBound < tester ) ||
                 ( lowerBound > tester ) ) {
                return false;
            } // testing integer bounds for a parsed to C# integer
            return true;
        }
        //
        public bool isValid( double tester )
        {
            if ( tester != tester ) { // nan test
                return false;
            }
            if ( possibleDoubleValues.Count > 0 ) {
                if ( !possibleDoubleValues.Contains( tester ) ) {
                    return false;
                }
                return true;
            }
            if ( ( upperDBound < tester ) ||
                 ( lowerDBound > tester ) )
            {
                return false;
            } // testing integer bounds for a parsed to C# integer
            return true;
        }
        //
        public bool isValid( string tester )
        {
            if ( string.IsNullOrEmpty( tester ) ) {
                return false;
            }
            if ( ( possibleStrValues.Count > 0 ) &&
                 ( !possibleStrValues.Contains( tester ) ) ) {
                return false;
            }
            return true;
        }
        //
        public override string ToString()
        {
            StringBuilder tmp = new StringBuilder();
            tmp.Append( "typeVal: " + typeVal + Environment.NewLine );
            tmp.Append( "type: " + type + Environment.NewLine );
            tmp.Append( "upperBound: " + upperBound + Environment.NewLine );
            tmp.Append( "lowerBound: " + lowerBound + Environment.NewLine );
            tmp.Append( "upperDBound: " + upperDBound + Environment.NewLine );
            tmp.Append( "lowerDBound: " + lowerDBound + Environment.NewLine );
            tmp.Append( "grouping: " + grouping + Environment.NewLine );
            foreach( double i in possibleDoubleValues ) {
                tmp.Append( "possibleDoubleValues:" + i + Environment.NewLine );
            }
            foreach( int i in possibleIntValues ) {
                tmp.Append( "possibleIntValues:" + i + Environment.NewLine );
            }
            foreach( string i in possibleStrValues ) {
                tmp.Append( "possibleStrValues:" + i + Environment.NewLine );
            }
            tmp.Append( "isBool: " + isBool + Environment.NewLine );
            return tmp.ToString();
        }
        //
        public bool Equals( Constraint obj )
        {
            if ( ( typeVal == obj.typeVal ) &&
                 ( type == obj.type ) &&
                 ( upperBound == obj.upperBound ) &&
                 ( lowerBound == obj.lowerBound ) &&
                 ( upperDBound == obj.upperDBound ) &&
                 ( lowerDBound == obj.lowerDBound ) &&
                 ( grouping == obj.grouping ) &&
                 ( isBool == obj.isBool ) &&
                 ( possibleDoubleValues.SequenceEqual( obj.possibleDoubleValues ) ) &&
                 ( possibleIntValues.SequenceEqual( obj.possibleIntValues ) ) &&
                 ( possibleStrValues.SequenceEqual( obj.possibleStrValues ) ) )
            {
                return true;
            }
            return false;
        }
        //
    } // end Constraint

    class InputObjectConstraint
    {
        //#TODO Constraint container and helpers
    }
}
