using System;
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
using CodeRayWpfLib;

// Copyright Richard Fencel Software (c) 2014

namespace CodeRayTestSimpleListBoxWpfApp.Views
{
    /// <summary>
    /// Interaction logic for Edit.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();

            DataContextChanged += _DataContextChanged;
            Unloaded += _Unloaded;
            TargetUpdated += _TargetUpdated;
            SourceUpdated += _SourceUpdated;
        }

        // see note in MainWindowViewModel.xaml.cs
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
            CodeRayDlg.RemoveElement(this);
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
    }
}
