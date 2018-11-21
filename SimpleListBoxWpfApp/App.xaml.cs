using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CodeRayWpfLib;

// Copyright Richard Fencel Software (c) 2014
using CodeRayTestSimpleListBoxWpfApp.ViewModels;
using CodeRayTestSimpleListBoxWpfApp.Views;

namespace SimpleListBoxWpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
           
            var viewModel = new MainWindowViewModel(null);

            var window = new MainWindowView();
            window.DataContext = viewModel;
            window.Show();

            CodeRayDlg.CodeRayShow();
        }
    }
}
