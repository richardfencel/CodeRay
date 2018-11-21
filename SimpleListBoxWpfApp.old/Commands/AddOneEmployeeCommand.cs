using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

// Copyright Richard Fencel Software (c) 2014
using CodeRayTestSimpleListBoxWpfApp.ViewModels;

namespace CodeRayTestSimpleListBoxWpfApp.Commands
{
    public class AddOneEmployeeCommand : ICommand
    {
        MainWindowViewModel _vm;

        public AddOneEmployeeCommand(MainWindowViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _vm.AddOneEmployee();
        }
    }
}
