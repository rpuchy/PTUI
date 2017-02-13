using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Data;
using PlanningTool;
using TreeViewWithViewModelDemo.TextSearch;

namespace BusinessLib
{
    /// <summary>
    /// A simple data transfer object (DTO) that contains raw data about a engine object.
    /// </summary>
    public class EngineObject
    {
        IList<EngineObject> _children = new List<EngineObject>();
        ParamList _parameters = new ParamList();
           
        public IList<EngineObject> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        public ParamList Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public EngineObject()
        {
            
        }

        public EngineObject(EngineObjectViewModel _engineObjectViewModel)
        {
            Name = _engineObjectViewModel.Name;
            NodeName = _engineObjectViewModel.NodeName;
            Parameters = _engineObjectViewModel.Parameters;
            Children = new List<EngineObject>(
                   (from child in _engineObjectViewModel.Children
                    select new EngineObject(child))
                    .ToList<EngineObject>());
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

        public EngineObject FindObject(string tagName)
        {
            if (NodeName == tagName)
            {
                return this;
            }
            foreach (var engineObject in this.Children)
            {
                var _obj = engineObject.FindObject(tagName);
                if (_obj != null)
                {
                    return _obj;
                }
            }
            return null;
        }

        public EngineObject FindObject(string value, string parameter)
        {
            if (Parameters[parameter]?.ToString() == value)
            {
                return this;
            }
            foreach (var engineObject in Children)
            {
                var _obj = engineObject.FindObject(value, parameter);
                if (_obj != null)
                {
                    return _obj;
                }
            }
            return null;
        }

        public List<EngineObject> FindObjects(string value, string parameter)
        {
            List<EngineObject> temp = new List<EngineObject>();
            if (Parameters[parameter].ToString() == value)
            {
                temp.Add(this);
            }
            foreach (var engineObject in Children)
            {
                var _obj = engineObject.FindObjects(value, parameter);
                temp.AddRange(_obj);
            }
            return temp;

        }

        public List<EngineObject> FindObjects(string tagname)
        {
            List<EngineObject> temp = new List<EngineObject>();
            if (this.NodeName == tagname)
            {
                temp.Add(this);
            }
            foreach (var engineObject in Children)
            {
                var _obj = engineObject.FindObjects(tagname);
                temp.AddRange(_obj);
            }
            return temp;

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

        public object Value
        {
            get { return (object)GetValue(_value); }
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