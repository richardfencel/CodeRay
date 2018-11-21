using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CodeRayWpfLib;

// Copyright Richard Fencel Software (c) 2014
using CodeRayTestSimpleListBoxWpfApp.Commands;
using CodeRayTestSimpleListBoxWpfApp.VieweModels;

namespace CodeRayTestSimpleListBoxWpfApp.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _companyName;

        public string CompanyName
        {
            get { return _companyName; }
            set
            {
                _companyName = value;
                OnPropertyChanged("CompanyName");
            }
        }

        private int _numberOfEmployees;

        public int NumberOfEmployees
        {
            get { return _numberOfEmployees; }
            set
            {
                _numberOfEmployees = value;
                OnPropertyChanged("NumberOfEmployees");
            }
        }

        private ObservableCollection<EmployeeViewModel> _employees;

        public ObservableCollection<EmployeeViewModel> Employees
        {
            get { return _employees; }
            set
            {
                _employees = value;
                OnPropertyChanged("Employees");
            }
        }

        public AddOneEmployeeCommand AoeCommand { get; set; }
        public RemoveOneEmployeeCommand RoeCommand { get; set; }

        public MainWindowViewModel(object parent)
        {
            AoeCommand = new AddOneEmployeeCommand(this);
            RoeCommand = new RemoveOneEmployeeCommand(this);
            Employees = new ObservableCollection<EmployeeViewModel>();
            
            // Add the view model to CodeRay.  Note:  we do this here rather than in the caller because if we do this in the caller after
            // we "new" this object, then this object will not have been added to the Dictionary when we try to add all the sub-objects in this class.
            CodeRayDlg.AddViewModel(this, parent);

            // add the collection of view models to CodeRay.
            CodeRayDlg.AddViewModelCollection(Employees, this, () => Employees);

            // create 10 Employee view models and add them to the Employees List 
            for (int i = 0; i < 10; ++i)
            {
                AddOneEmployee();
            }
        }

        public void AddOneEmployee()
        {
            int index = Employees.Count + 1;

            string name = "John Smith" + index;

            var vm = new EmployeeViewModel(Employees, name) {Name = name, JobTitle = "janitor" + index, Id = "Emp" + index.ToString()};
           
            Employees.Add(vm);

            NumberOfEmployees = Employees.Count();
        }

        public void RemoveOneEmployee()
        {
            if (Employees.Count > 0)
            {
                CodeRayDlg.RemoveElement(Employees[Employees.Count - 1]);
                Employees.RemoveAt(Employees.Count - 1);

                NumberOfEmployees = Employees.Count();
            }
        }
    }
}
