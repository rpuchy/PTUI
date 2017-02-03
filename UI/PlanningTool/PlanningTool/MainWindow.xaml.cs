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
using System.Collections.Specialized;
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

        private FileOpsImplementation fOps;
        private TreeViewModel VisualData;
        private EngineObject Data;


        readonly ObservableCollection<Parameter> _parameters = new ObservableCollection<Parameter>();

        public MainWindow()
        {
            InitializeComponent();
            TreeviewControl.EngineObjectViewTree.SelectedItemChanged += new RoutedPropertyChangedEventHandler<Object>(InterfaceTreeViewComputers_SelectionChange);
            base.DataContext = this;
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
            if (openFileDialog1.ShowDialog() == true)
            {
                string fileName = openFileDialog1.FileName;
                fOps = new FileOpsImplementation(fileName);
            }
            Data = fOps.EngineObjectTree;
            VisualData = new TreeViewModel(Data);
            TreeviewControl.SetData(VisualData);
            
            base.DataContext = TreeviewControl;
            listView.DataContext = this;            
        }

        void InterfaceTreeViewComputers_SelectionChange(Object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //first write back the parameters then move to the next
            if (e.OldValue != null)
            {
                ((EngineObjectViewModel) e.OldValue).Parameters.Clear();

                foreach (var param in _parameters)
                {
                    ((EngineObjectViewModel) e.OldValue).Parameters.Add(new Parameter()
                    {
                        Name = param.Name,
                        Value = param.Value
                    });
                }
            }
            _parameters.Clear();
            //We need to check if this is a table i.e. all the children have the same name 
            //TODO : move this to a property of the model
            bool table = false;
            /*if (((EngineObjectViewModel) e.NewValue).Children.Count > 1)
            {
                table = true;
                string name = ((EngineObjectViewModel)e.NewValue).Children[0].Name;
                foreach (var child in ((EngineObjectViewModel)e.NewValue).Children)
                {
                    if ((name != child.Name)  ) table = false;
                }
            }*/

            if (table)
            {
                //if it's a table we change the parameter names and show more/fewer columns
                foreach (var child in ((EngineObjectViewModel) e.NewValue).Children)
                {
                    _parameters.Add(new Parameter() {Name=child.Parameters[0].Value, Value = child.Parameters[1].Value});
                }
            }
            else
            {
                foreach (var param in ((EngineObjectViewModel) e.NewValue).Parameters)
                {
                    _parameters.Add(new Parameter() {Name = param.Name, Value = param.Value});
                }
            }

            AddressBox.Text = ((EngineObjectViewModel) e.NewValue).Fullyqualifiedname;
        }

        private bool fileisopen()
        {
            return fOps!=null ;
        }

        private void listViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            object obj = item.Content;
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            fOps.UpdateModel(VisualData.FirstGeneration[0]);
            fOps?.Save();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (fOps != null)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Document"; // Default file name
                dlg.DefaultExt = ".xml"; // Default file extension
                dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;
                    //write the viewModel back to the main object then save
                    fOps.UpdateModel(VisualData.FirstGeneration[0]);
                    fOps.SaveAs(filename);
                }
            }
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            if (fOps != null)
            {
                fOps.AddAlloutputs(0, 100);                
                VisualData = new TreeViewModel(fOps.EngineObjectTree);
                TreeviewControl.SetData(VisualData);
            }
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            if (fOps != null)
            {
                fOps.Removealloutputs();
                VisualData = new TreeViewModel(fOps.EngineObjectTree);
                TreeviewControl.SetData(VisualData);
            }
        }
    }


    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            bool param = bool.Parse(parameter as string);
            bool val = (bool)value;

            return val == param ?
              Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
