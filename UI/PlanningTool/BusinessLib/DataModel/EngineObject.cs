using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Data;

namespace BusinessLib
{
    /// <summary>
    /// A simple data transfer object (DTO) that contains raw data about a engine object.
    /// </summary>
    public class EngineObject
    {
        IList<EngineObject> _children = new List<EngineObject>();
        IList<Parameter> _parameters = new List<Parameter>();
           
        public IList<EngineObject> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        public IList<Parameter> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string Name { get; set; }

        public string NodeName { get; set; }

        public void AddEngineObjectChild(EngineObject child)
        {
            _children.Add(child);
        }

        public void AddParameter(Parameter _param)
        {
            _parameters.Add(_param);
        }
    }

    public class Parameter: DependencyObject, INotifyPropertyChanged, IEquatable<Parameter>
    {
        public Parameter()
        {
            
        }


        public bool Equals(object obj)
        {
            if (obj == null) return false;
            Parameter objAsPart = obj as Parameter;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public bool Equals(Parameter other)
        {
            if (other == null) return false;
            return (this.Name.Equals(other.Name)&&this.Value.Equals(other.Value));
        }

        public static readonly DependencyProperty _name =
        DependencyProperty.Register("Name", typeof(string),
        typeof(Parameter), new UIPropertyMetadata(null));

        public string Name
        {
            get { return (string)GetValue(_name); }
            set { SetValue(_name, value); }
        }

        public static readonly DependencyProperty _value =
        DependencyProperty.Register("Value", typeof(string),
        typeof(Parameter), new UIPropertyMetadata(null));

        public string Value
        {
            get { return (string)GetValue(_value); }
            set { SetValue(_value, value); }
        }

       

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string Obj)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(Obj));
            }
        }

    }

}