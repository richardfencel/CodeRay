using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
// need this for PropertyChangedEventHandler
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CodeRayWpfLib;

// Copyright Richard Fencel Software (c) 2014
using CodeRayTestSimpleListBoxWpfApp.ViewModels;

namespace CodeRayTestSimpleListBoxWpfApp.VieweModels
{
    public class EmployeeViewModel : BaseViewModel
    {
        private string _name;
        private string _jobTitle;
        private string _id;
        
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string JobTitle
        {
            get { return _jobTitle; }
            set
            {
                _jobTitle = value;
                OnPropertyChanged("JobTitle");
            }
        }

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
       
        public EmployeeViewModel(object parent, string name)
        {
            // we use constructor args rather than the properties because the properties are not yet initialized by the object initializer at constructor time
            CodeRayDlg.AddViewModel(this, parent, name);
        }
    }
}
