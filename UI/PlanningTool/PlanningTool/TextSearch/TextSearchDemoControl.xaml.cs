using System;
using System.Diagnostics.PerformanceData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BusinessLib;
using PlanningTool;
using RequestRepresentation;

namespace TreeViewWithViewModelDemo.TextSearch
{
    public partial class TextSearchDemoControl : UserControl
    {
        readonly TreeViewModel _tree;  

        private EngineObject _engineObjectTree = new EngineObject();

        public EngineObject EngineObjectTree
        {
            get { return _engineObjectTree; }
            set { _engineObjectTree = value; }
        }

        public TextSearchDemoControl()
        {
            InitializeComponent();
            
            // Get raw family tree data from a database.
            EngineObject rootObject = EngineObjectTree;

            // Create UI-friendly wrappers around the 
            // raw data objects (i.e. the view-model).
            _tree = new TreeViewModel(rootObject);

            // Let the UI bind to the view-model.
            base.DataContext = _tree;            
        }

        public void SetData(TreeViewModel data)
        {
            base.DataContext = data;
        }

        void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ((TreeViewModel)base.DataContext).SearchCommand.Execute(null);
        }
    }
}