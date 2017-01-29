using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

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

        public void AddEngineObjectChild(EngineObject child)
        {
            _children.Add(child);
        }

        public void AddParameter(Parameter _param)
        {
            _parameters.Add(_param);
        }
    }

    public class Parameter:INotifyPropertyChanged
    {
        public Parameter()
        {
            
        }
        private string _name;
        private string _value;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyPropertyChanged("Value");
            }
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