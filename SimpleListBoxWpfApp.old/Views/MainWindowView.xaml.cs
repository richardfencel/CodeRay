using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
using CodeRayWpfLib;
using CodeRayTestSimpleListBoxWpfApp.Commands;

// Copyright Richard Fencel Software (c) 2014

namespace CodeRayTestSimpleListBoxWpfApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();

            DataContextChanged += _DataContextChanged;
            Unloaded += _Unloaded;
            TargetUpdated += _TargetUpdated;
            SourceUpdated += _SourceUpdated;
        }

        // This handles both the case when the app creates the view each time it creates a view model and also the case when the app creates a new view model, it keeps the view and 
        // merely changes its DataContext (if we were handling only the first case, we would put this logic in the the need to handle the Loaded event).
        void _DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // need to check for null because for some reason this event may fire with DataContext = null
            if (DataContext != null)
            {
                // remove this view with the old DataContext
                CodeRayDlg.RemoveElement(this);

                // add this view with the new DataContext
                CodeRayDlg.AddView(this);
            }
        }

        void _Unloaded(object sender, RoutedEventArgs e)
        {
            CodeRayDlg.CloseCodeRay();
        }

        void _SourceUpdated(object sender, DataTransferEventArgs e)
        {
            CodeRayDlg.SaveBindingData(sender, e, this, Update.Source);
            e.Handled = true;
        }

        void _TargetUpdated(object sender, DataTransferEventArgs e)
        {
            CodeRayDlg.SaveBindingData(sender, e, this, Update.Target);
            e.Handled = true;
        }

        // No longer used, was test code for expression trees.
        //void MainWindowView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var domain = "matrix";
        //    Check(() => domain);
        //    CodeRayDlg.GetMemberName(() => domain);

        //    Tag = "hello";
        //    CodeRayDlg.GetMemberName(() => this);
        //    MyListBox.Tag = "hello";
       //     Test = "goodbye";
       //     CodeRayDlg.GetMemberName(() => domain);
       //     CodeRayDlg.GetMemberName(() => Test);

       //     CodeRayDlg.GetMemberName(() => MyListBox.Tag);

       //     CodeRayDlg.AddView(this);
      //  }
    }
}
