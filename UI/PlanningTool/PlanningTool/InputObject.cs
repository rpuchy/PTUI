using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RequestRepresentation
{
    //
    public class scalar_t
    {
        public string   name                { get; set; }
        public string   type                { get; set; }
        public string   depends             { get; set; }
        public string   subType             { get; set; }
        public string   metaType            { get; set; }
        public string   reference           { get; set; }
        public string   referenceField      { get; set; }
        public string   referenceSubType    { get; set; }
        public string   referenceMetaType   { get; set; }
        public int      minOccurs           { get; set; }
        public int      maxOccurs           { get; set; }
        public string   defaultValue        { get; set; }
        public string   defaulttype         { get; set; }
        public bool     isUniqueScope       { get; set; }
        public string   uniqueScopeTo       { get; set; }
        public string   desc                { get; set; }
        public Constraint constraint        { get; set; }
        //
        public scalar_t()
        {
            //constraint = new Constraint();
        }
        //
        public scalar_t( string _name, string _type, int _min, int _max
                        ,string _depends, string _def, string _desc )
        {
            name                = _name;
            type                = _type;
            depends             = _depends;
            minOccurs           = _min;
            maxOccurs           = _max;
            defaultValue        = _def;
            desc                = _desc;
        }
        //
        public scalar_t( string _name, string _type, int _min, int _max
                        ,string _depends, string _def, string _desc
                        ,Constraint _constraint )
        {
            name                = _name;
            type                = _type;
            depends             = _depends;
            minOccurs           = _min;
            maxOccurs           = _max;
            defaultValue        = _def;
            desc                = _desc;
            constraint          = _constraint;
        }
        //
        //public scalar_t( string _name, string _type, int _min, int _max
        //                ,string _depends, string _def, string _desc
        //                ,string type_constraint )
        //{
        //    name                = _name;
        //    type                = _type;
        //    depends             = _depends;
        //    minOccurs           = _min;
        //    maxOccurs           = _max;
        //    defaultValue        = _def;
        //    desc                = _desc;
        //    constraint          = new Constraint( ref type_constraint );
        //}
        //
        public scalar_t( ref scalar_t _src )
        {
            name                = _src.name;
            type                = _src.type;
            depends             = _src.depends;
            subType             = _src.subType;
            metaType            = _src.metaType;
            reference           = _src.reference;
            referenceField      = _src.referenceField;
            referenceSubType    = _src.referenceSubType;
            minOccurs           = _src.minOccurs;
            maxOccurs           = _src.maxOccurs;
            defaultValue        = _src.defaultValue;
            defaulttype         = _src.defaulttype;
            isUniqueScope       = _src.isUniqueScope;
            uniqueScopeTo       = _src.uniqueScopeTo;
            desc                = _src.desc;
            constraint          = _src.constraint;
        }

        //#TODO find relevant constraint and add link to it

        //
        //void Add( string _in )
        //{
        //    constraints.Add( _in );
        //}
        ////
        //void Remove( string _out )
        //{
        //    constraints.Remove( _out );
        //}
        ////
        //List<string> Get( Predicate<string> _incomplete )
        //{
        //    return constraints.FindAll( _incomplete );
        //}
        ////
        //string Get( int idx )
        //{
        //    return constraints[ idx ];
        //}
        ////
        //string Get( string TypeName )
        //{
        //    return constraints.Find( e => e == TypeName );
        //}
    }
    //
    //
    //
    public class complex_t
    {
        public string Name { get; set; }
        //
        public List<scalar_t> ObjElements;
        //
        public complex_t( string _name )
        {
            Name = _name;
            ObjElements = new List<scalar_t>();
        }
        //
        void Add( scalar_t _in )
        {
            ObjElements.Add( _in );
        }
        //
        void Remove( scalar_t _out )
        {
            ObjElements.Remove( _out );
        }
        //
        List<scalar_t> Get( Predicate<scalar_t> _incomplete )
        {
            return ObjElements.FindAll( _incomplete );
        }
        //
        scalar_t Get( int idx )
        {
            return ObjElements[ idx ];
        }
        //
        scalar_t Get( string TypeName )
        {
            return ObjElements.Find( e => e.type == TypeName );
        }
    }
    //
    public class InputElementContainer
    {
        public List<string> typeList;
        public List<complex_t> ObjContainer;
        //
        public InputElementContainer() {
            typeList = new List<string>();
            ObjContainer = new List<complex_t>();
        }
        //
        void Add( complex_t _in ) {
            ObjContainer.Add( _in );
        }
        //
        void Remove( complex_t _out ) {
            ObjContainer.Remove( _out );
        }
        //
        List<complex_t> Get( Predicate<complex_t> _incomplete ) {
            return ObjContainer.FindAll( _incomplete );
        }
        //
        complex_t Get( int idx ) {
            return ObjContainer[ idx ];
        }
        //
        complex_t Get( string ObjName ) {
            return ObjContainer.Find( e => e.Name == ObjName );
        }
        //
    }
    //
}
