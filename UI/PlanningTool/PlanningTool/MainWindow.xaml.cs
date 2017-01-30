﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using BusinessLib;
using Microsoft.Win32;
using RequestRepresentation;
using TreeViewWithViewModelDemo.TextSearch;

namespace PlanningTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private FileOpsImplementation fOps = new FileOpsImplementation();
        private TreeViewModel Data;


        readonly ObservableCollection<Parameter> _parameters = new ObservableCollection<Parameter>();

        public MainWindow()
        {
            InitializeComponent();
            TreeviewControl.EngineObjectViewTree.SelectedItemChanged += new RoutedPropertyChangedEventHandler<Object>(InterfaceTreeViewComputers_SelectionChange);
          
        }

        public ObservableCollection<Parameter> Parameters
        {
            get { return _parameters; }
        }

        public EngineObject DataTree()
        {
            return fOps.EngineObjectTree;
        }

    
        private void Window_Initialized(object sender, EventArgs e)
        {

        
        }
        
        public class MenuItem
        {
            public MenuItem()
            {
                this.Items = new ObservableCollection<MenuItem>();
            }

            public string Title { get; set; }

            public ObservableCollection<MenuItem> Items { get; set; }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            fOps = new FileOpsImplementation();
            if (openFileDialog1.ShowDialog() == true)
            {
                string fileName = openFileDialog1.FileName;
                fOps.ProcessFile(fileName);
            }
            Data = new TreeViewModel(fOps.EngineObjectTree);
            TreeviewControl.SetData(Data);
            
            base.DataContext = TreeviewControl;
            listView.DataContext = this;
            

        }
        void InterfaceTreeViewComputers_SelectionChange(Object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _parameters.Clear();
            foreach (var param in ((EngineObjectViewModel)e.NewValue).Parameters)
            {
                _parameters.Add(new Parameter() { Name = param.Name, Value = param.Value });
            }
            AddressBox.Text = ((EngineObjectViewModel) e.NewValue).Fullyqualifiedname;
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            object obj = item.Content;
        }
    }
}
