using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows.Shapes;

// Copyright Richard Fencel Software (c) 2014

namespace CodeRayWpfLib
{
    
    // Indicates whether we updated the source or the target
    public enum Update
    {
        Source,
        Target
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CodeRayDlg : Window
    {
        // This is the ItemsSource of the tree, specifically a list of nodes.  This list contains only one item, specifically 
        // the root node.  However, this node as well as the other nodes has a Children property that points to
        // subnode (all nodes are stored in the Dictionary _nodes) Because the tree uses a HierarchialDataTemplate that keys off the 
        // Children property, the entire tree is constructed from this one single item. 
        public static ObservableCollection<Node> _treeNodes = new ObservableCollection<Node>();

        // This is a Dictionary of all Nodes indexed by the address of the object that the Node is based on.  This object can be either a view, 
        // view model or view model collection. 
        public static Dictionary<object, Node> _nodes = new Dictionary<object, Node>();

        static private CodeRayDlg _dlg ;
        private Visual _previousViewVisualElement = null;
        private Adorner _previousViewAdorner = null;
        private Visual _previousTargetObjVisualElement = null;
        private Adorner _previousTargetObjectAdorner = null;

        // This contains all the Binding updates.  Each record, i.e. Binding update, is displayed as one line in the DataGrid.
        public static ObservableCollection<BindingRecord> _records = new ObservableCollection<BindingRecord>();

        public CodeRayDlg()
        {
            InitializeComponent();

            Loaded += CodeRayDlg_Loaded;
            _myTreeView.SelectedItemChanged += _myTreeView_SelectedItemChanged;
            
            _records.CollectionChanged += _records_CollectionChanged;

            Records.SelectionChanged += Records_SelectionChanged;
            
            // Initialize the DataGrid

            Records.ItemsSource = _records;

            Records.AutoGenerateColumns = false;
            DataGridTextColumn col1 = new DataGridTextColumn() { Header = "Source ViewModel", Binding = new Binding("SourceViewModel") };
            DataGridTextColumn col2 = new DataGridTextColumn() { Header = "Source Property", Binding = new Binding("SourceProperty") };
            DataGridTextColumn col3 = new DataGridTextColumn() { Header = "Target View", Binding = new Binding("TargetView") };
            DataGridTextColumn col4 = new DataGridTextColumn() { Header = "Target Object", Binding = new Binding("TargetObject") };
            DataGridTextColumn col5 = new DataGridTextColumn() { Header = "Target Property", Binding = new Binding("TargetProperty") };
            DataGridTextColumn col6 = new DataGridTextColumn() { Header = "Value", Binding = new Binding("Value") };
            DataGridTextColumn col7 = new DataGridTextColumn() { Header = "Direction", Binding = new Binding("UpdateDirection") };
            DataGridTextColumn col8 = new DataGridTextColumn() { Header = "Mode", Binding = new Binding("Mode") };
            DataGridTextColumn col9 = new DataGridTextColumn() { Header = "Binding Group Name", Binding = new Binding("BindingGroupName") };
            DataGridTextColumn col10 = new DataGridTextColumn() { Header = "Delay", Binding = new Binding("Delay") }; 
            DataGridTextColumn col11 = new DataGridTextColumn() { Header = "Fallback Value", Binding = new Binding("FallbackValue") };
            DataGridTextColumn col12 = new DataGridTextColumn() { Header = "Format", Binding = new Binding("Format") };
            DataGridTextColumn col13 = new DataGridTextColumn() { Header = "Target Null Value", Binding = new Binding("TargetNullValue") };

            Records.Columns.Add(col1);
            Records.Columns.Add(col2);
            Records.Columns.Add(col3);
            Records.Columns.Add(col4);
            Records.Columns.Add(col5); 
            Records.Columns.Add(col6);
            Records.Columns.Add(col7);
            Records.Columns.Add(col8);
            Records.Columns.Add(col9);
            Records.Columns.Add(col10);
            Records.Columns.Add(col11);
            Records.Columns.Add(col12);
            Records.Columns.Add(col13);
        }

        /*==================================================================================
                                                 API
         
        The API calls need to be static because they can be called from anywhere in the client
        ==================================================================================*/

        // add a view to the tree
        static public void AddView(FrameworkElement view)
        {
            var parent = view.DataContext;
            var node = new Node(view, ElementType.View);

            // get the leaf name. 

            string viewName = null;

            if (view.Name != "")
            {
                // todo: check if FrameworkElement or FrameworkContentElement, see LogicalNode.cs in DRay
                viewName = ((FrameworkElement)view).Name;
            }

            // get the typename after the last period, i.e. convert MyNamespace.SubNameSpace.MyClassName to MyClassName
            string typeName = view.GetType().ToString();
            node.LeafName = GetLastSyllable(typeName);

            if (viewName != null)
            {
                node.LeafName += "(" + viewName + ")";
            }

            AddNodeToDictionary(node, view, parent);
        }

        // adds a view model to the tree.
        static public void AddViewModel(object viewModel, object parent, string propertyValue = null)
        {
            var node = new Node(viewModel, ElementType.ViewModel);

           // get the typename after the last period, i.e. convert MyNamespace.SubNameSpace.MyClassName to MyClassName
            string typeName = viewModel.GetType().ToString();
            node.LeafName = GetLastSyllable(typeName);

            if (propertyValue != null)
            {
                node.LeafName += "(" + propertyValue + ")";
            }

            AddNodeToDictionary(node, viewModel, parent);
        }

        // Add a view model collection to the tree.  We always require that the user specify the source variable name when adding a collection. 
        // It makes no sense to specify a property on the collection and it also makes no sense to specify the type of the collection (in the latter
        // case we get some ungodly thing like:  System.Collections.ObjectModel.ObservableCollection`1[CodeRayTestSimpleListBox.VieweModels.EmployeeViewModel]).
        static public void AddViewModelCollection<T>(IEnumerable collection, object parent, Expression<Func<T>> variable)
        {
            var node = new Node(collection, ElementType.ViewModelCollection);
            node.LeafName = GetMemberName(variable);

            AddNodeToDictionary(node, collection, parent);
        }

        // Shutdown the codeRay utility 
        static public void CloseCodeRay()
        {
            ((Window)_dlg).Close();
        }

        // Invoke the CodeRay utility
        static public void CodeRayShow()
        {
            _dlg = new CodeRayDlg();
            ((Window)_dlg).Show();
        }

        // Everytime a binding update (i.e. "source updated" or "target updated") occurs, this method is called. This method creates a "update" record of type BindingRecord
        // and adds this record to _records which is the ItemsSource of the DataGrid. 
        public static void SaveBindingData(object sender, DataTransferEventArgs e, DependencyObject view, Update update)
        {
            // todo: add the "Name" in parentheses to TargetView and TargetObject (if TargetObject is a view) and the property in parentheses to the viewmodel

            var s = (DependencyObject)sender;
            var fe = e.TargetObject as FrameworkElement;

            BindingExpression be = fe.GetBindingExpression(e.Property);

            if (be == null)
            {
                string message = "GetBindingExpression returns null even though the Binding operation was successful.  This is an anomaly that was seen when a Binding was specified inside a DataTemplate that was inside a CellTemplate. ";
                message += "As a worksaround, try eliminating nested templates";
            }
            else
            {
                if (be.ResolvedSource != null && be.ResolvedSourcePropertyName != null)
                {
                    Binding binding = BindingOperations.GetBinding(s, e.Property);
                    BindingRecord br = new BindingRecord();

                    // get the source name and the source object
                    if (be.ResolvedSource != null)
                    {
                        // to do:  in certain cases, be.ResolvedSource.ToString() can be null even though be.ResolveSource is a real object
                        // (we saw this with MenuItemInfo).  We should therefore use reflection here to determine the object's name.

                        string temp = be.ResolvedSource.ToString();

                        if (temp != null)
                        {
                            br.SourceViewModel = GetLastSyllable(temp);
                            br.SourceViewModelObj = be.ResolvedSource;
                        }
                    }

                    if (be.ResolvedSourcePropertyName != null)
                    {
                        br.SourceProperty = be.ResolvedSourcePropertyName;
                    }

                    // get the target object and name
                    if (be.Target != null)
                    {
                        br.TargetObject = be.Target.GetType().Name;
                        br.TargetObjectObj = be.Target;

                        if (be.Target is FrameworkElement)
                        {
                            string name = ((FrameworkElement)be.Target).Name;

                            if (name != "")
                            {
                                br.TargetObject += "(" + name + ")";
                            }
                        }
                    }

                    // get the target property and value
                    if (be.TargetProperty != null)
                    {
                        br.TargetProperty = be.TargetProperty.Name;

                        if (be.Target != null)
                        {
                            object value = ((DependencyObject)be.Target).GetValue(be.TargetProperty);

                            if (value != null)
                            {
                                if (value is ICollection)
                                {
                                    br.Value = "ICollection";
                                }
                                else
                                {
                                    br.Value = value.ToString();
                                }
                            }
                            else
                            {
                                br.Value = "Null";
                            }
                        }
                    }

                    br.UpdateDirection = update;

                    // get the view and view name in which the target resides
                    if (view is FrameworkElement)
                    {
                        string temp = view.ToString();
                        br.TargetView = GetLastSyllable(temp);

                        string name = ((FrameworkElement)view).Name;

                        if (name != "")
                        {
                            br.TargetView += "(" + name + ")";
                        }
                    }

                    br.TargetViewObj = view;

                    _records.Add(br);

                    if (_dlg != null)
                    {
                        // always show the last record added
                        _dlg.Records.ScrollIntoView(_dlg.Records.Items.GetItemAt(_dlg.Records.Items.Count - 1));
                    }
                }
            }
        }

        /*==================================================================================
                                              API HELPER METHODS
        ==================================================================================*/
        
        // Removes an element from the Dictionary and the tree

        // This method is passed an element that can be either a view, viewmodel or viewmodel collection.
        // This method then fetches from the Dictionary the element's corresponding Node and then uses the 
        // Node to do three things:
        
        // (1) Removes itself from its parent's list of Children

        // (2) Removes all its Children from the Dictionary
        
        // (3) Removes itself from the Dictionary
        
        // Note: the corresponding Node may not exist because it may have already been deleted (e.g. when the 
        // Unloaded event is fired in the view, the Node may not be found because when the corresponding view model 
        // was removed from a viewmodel collection, this method was called and the view model Node and its Children 
        // were deleted). 

        // When all pointers to this element have been removed from other elements in the Dictionary, this element will
        // automatically be removed from the tree because of the HierarchicalDataTemplate.
        static public void RemoveElement(object element)
        {
            // First, get the node that corresponds to the element.  Note that the node may not be
            // found because it could already be deleted.

            Node node;
            
            if (_nodes.TryGetValue(element, out node) == true)
            {
                if (node.Parent != null)
                {
                    // remove the node from its parent's list of Children
                    node.Parent.Children.Remove(node);
                }

                // recursively remove the node's children from the Dictionary
                RemoveChildren(node.Children);

                _nodes.Remove(element);
            }
        }

        // recursive method to remove from the Dictionary all Children of a particular node
        static private void RemoveChildren(ObservableCollection<Node> children)
        {
            for (int i = children.Count - 1; i >= 0; --i)
            {
                RemoveChildren(children[i].Children);
                _nodes.Remove(children[i].Element);
            }
        }
        
        // Adds a node to the Dictionary and adds to its parent's Children a reference to itself.  If the node is the
        // root node, then it is added to the _treeNodes list.
        static public void AddNodeToDictionary(Node node, object element, object parent)
        {
            try
            {
                // is this the root node?
                if (parent == null)
                {
                    // yes, this node becomes the first and only entry in _treeNodes  
                    _treeNodes.Add(node);
                }
                else
                {
                    // no, add this node to the parent's children 

                    Node parentNode;

                    // get the parent node
                    if (_nodes.TryGetValue(parent, out parentNode) == true)
                    {
                        // add to parent node's children
                        parentNode.Children.Add(node);

                        // also save the parent node in the node we are adding to the Dictionary
                        node.Parent = parentNode;
                    }
                }

                // add this node to the Dictionary of all Nodes
                _nodes.Add(element, node);
            }
            catch (Exception Ex)
            {
                string message = string.Format("Error while tring to add element of type {0} to Dictionary: ", element.GetType()) + Ex.Message;
                MessageBox.Show(message);
            }
        }

        // This method converts an expression that represents a variable into the variable's name used in compilation.  For example, if we pass
        // "() => Employees" to this method it returns the string "Employees".
        static public string GetMemberName<T>(Expression<Func<T>> expr)
        {
            var body = ((MemberExpression)expr.Body);
            return body.Member.Name;


            // For information only:

            // In advanced version of this product, we may have a need to determine not only the string name but also the value of the variable.
            // The code below is various experiments used to determine the value of the variable.  We would like to have this this so that if the variable
            // changes, we can update the tree leaf with the latest value (i.e. when we have "MyViewModel(John Smith") and the name changes to 
            // "Mike Wilson" we display "MyViewModel(Mike Wilson)".

            // See http://stackoverflow.com/questions/716399/c-sharp-how-do-you-get-a-variables-name-as-it-was-physically-typed-in-its-dec.  Using this article,
            // we implemented a version of Check() like the one based in the article:

            // static void Check<T>(Expression<Func<T>> expr)
            // {
            //    var body = ((MemberExpression)expr.Body);
            //    Console.WriteLine("Name is: {0}", body.Member.Name);
            //    Console.WriteLine("Value is: {0}", ((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value));
            // } 
            
            // This works with  CodeRayDlg.Check(() => Tag); but does not work with  CodeRayDlg.Check(() => domain);
            //   string s = (string) ((System.Reflection.PropertyInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value);

            // this does not work with  CodeRayDlg.Check(() => Tag); but works with CodeRayDlg.Check(() => domain);
            //
            //  string s1 =  (string) ((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value);

            // neither of the above work with   CodeRayDlg.Check(() => MyListBox.Tag);

            // NOTE: we must also deal with the fact that the property value may change so we must figure out what the binding for it is and
            // in the "updated" event handler we must get the new value and put the new value in the tree leaf.
        }

        // get the last syllable after the last period, i.e. convert MyNamespace.SubNameSpace.MyClassName to MyClassName
        private static string GetLastSyllable(string input)
        {
            return (input.Substring(input.LastIndexOf('.') + 1));
        }

        #if OMIT  
        // no longer used
        // removes "System.Windows.Controls." from the start of the string and removes anything after a space
        private static string RemovePrefixAndSuffix(string s)
        {
            string prefix = "System.Windows.Controls.";
            string temp = s;

            if (temp.StartsWith(prefix))
            {
                temp = temp.Substring(prefix.Length);
            }

            string[] temp2 = temp.Split(' ');
            return temp2[0];
        }
        #endif

        /*==================================================================================
                                              EVENT HANDLERS
        ==================================================================================*/
        void CodeRayDlg_Loaded(object sender, RoutedEventArgs e)
        {
            _myTreeView.ItemsSource = _treeNodes;
        }

        // This fires when we click on a record in the DataGrid.  If the target item is a view, we use an Adorner to put a Green border around the target object (e.g. a TextBlock inside the view).
        // We also select the view in the tree (which automatically puts a border around the tree).  For more info on Adorners see:
        // http://msdn.microsoft.com/query/dev12.query?appId=Dev12IDEF1&l=EN-US&k=k(System.Windows.Documents.AdornerLayer);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true
        void Records_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] is BindingRecord == true)
            {
                BindingRecord br = e.AddedItems[0] as BindingRecord;

                if (br.TargetObjectObj is FrameworkElement)
                {
                    var fe = br.TargetObjectObj as FrameworkElement;
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(fe);

                    // does this element have a adorner layer?
                    if (layer != null)
                    {
                        // yes, remove the previous Adorner and add one to the selected element
                        RemovePreviousTargetObjectAdorner();
                        Adorner ad = new RectangleAdorner(fe, Constants.TargetObjectColor);
                        layer.Add(ad);
                        SavePreviousTargetObject(fe, ad);
                    }
                    else if (fe is Window)
                    {

                        var win = fe as Window;

                        RemovePreviousTargetObjectAdorner();
                        AdornerLayer layer2 = AdornerLayer.GetAdornerLayer((Visual)win.Content);

                        Adorner ad = new RectangleAdorner((FrameworkElement)win.Content, Constants.TargetObjectColor);
                        layer2.Add(ad);

                        SavePreviousTargetObject((FrameworkElement)win.Content, ad);
                    }

                    else
                    {
                        // if we reach here, then the object has been deleted.  This happens when we scroll through a ListBox and the ListBox items get tossed.
                        MessageBox.Show("Target object not found");
                    }
                }
            }
        }

        // this event and discussion is provided for information only
        void _records_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // As an alternative to calling ScrollIntoView() in SaveBindingData(), we tried calling it below:

            //    Records.ScrollIntoView(Records.Items.GetItemAt(Records.Items.Count - 1));

            // However, this caused the exception "Cannot change ObservableCollection during a CollectionChanged event".  Apparently, the TargetUpdated 
            // event in the binding fired while we were in the middle  of the Collection_changed event.  This occurred when we rapidly scrolled the
            // ListBox and then repeatedly hit "Add One Record" and "Remove One Record"  (the reason we called ScrollIntoView() here is that 
            // in the static method SaveBindingData() we could not access the non-static variable Records.  However, we fixed this problem by 
            // accessing the static variable _dlg.Records.
        }

        // This logic fires whenever we select a tree leaf.  If the tree leaf represents a view, we put a ViewColor border around the view using an Adorner.  
        void _myTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;

            Node node = tree.SelectedItem as Node;

            // is the selected element a view?
            if (node.Element is FrameworkElement)
            {
                // yes, get the layer 
                var fe = node.Element as FrameworkElement;
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(fe);

                // does this element have a adorner layer?
                if (layer != null)
                {
                    // yes, remove the previous Adorner and add one to the selected element
                    RemovePreviousViewAdorner();
                    Adorner ad = new RectangleAdorner(fe, Constants.ViewColor);
                    layer.Add(ad);
                    SavePreviousView(fe, ad);
                }
                // no, is this element a Window?
                else if (fe is Window)
                {
                    // Yes, in the case of the window, there is apparently no Adorner layer (possibly because there is nothing above it).  Therefore, we place the ViewColor rectangle on the 
                    // Content of the window. Note: both Snoop and WPF Inspector apparently faced the same problem because they will not put a border on the top level window.
                    var win = fe as Window;

                    RemovePreviousViewAdorner();
                    AdornerLayer layer2 = AdornerLayer.GetAdornerLayer((Visual)win.Content);

                    Adorner ad = new RectangleAdorner((FrameworkElement)win.Content, Constants.ViewColor);
                    layer2.Add(ad);

                    SavePreviousView((FrameworkElement)win.Content, ad);
                }
                else
                {
                    MessageBox.Show(String.Format("Can't find Adorner layer for element of type {0}", fe.GetType()));
                }
            }
            else
            {
                RemovePreviousViewAdorner();
            }
        }

        /*==================================================================================
                                         EVENT HANDLER HELPER METHODS
        ==================================================================================*/
        private void RemovePreviousTargetObjectAdorner()
        {
            if (_previousTargetObjVisualElement != null)
            {
                AdornerLayer previousLayer = AdornerLayer.GetAdornerLayer(_previousTargetObjVisualElement);

                if (previousLayer != null)
                {
                    previousLayer.Remove(_previousTargetObjectAdorner);
                }
            }
        }

        private void SavePreviousTargetObject(Visual v, Adorner ad)
        {
            _previousTargetObjVisualElement = v;
            _previousTargetObjectAdorner = ad;
        }



        private void RemovePreviousViewAdorner()
        {
            if (_previousViewVisualElement != null)
            {
                AdornerLayer previousLayer = AdornerLayer.GetAdornerLayer(_previousViewVisualElement);

                if (previousLayer != null)
                {
                    previousLayer.Remove(_previousViewAdorner);
                }
            }
        }

        private void SavePreviousView(Visual v, Adorner ad)
        {
            _previousViewVisualElement = v;
            _previousViewAdorner = ad;
        }
    }
}
