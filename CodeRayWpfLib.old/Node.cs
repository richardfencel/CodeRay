using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

// Copyright Richard Fencel Software (c) 2014

namespace CodeRayWpfLib
{
    public enum ElementType
    {
        View,
        ViewModel,
        ViewModelCollection
    }

    // This class describes one node in the view - view model - view model collection tree.  All nodes are stored in
    // the Dictionary _nodes in CodeRayDlg.xaml.cs.
    public class Node
    {
        // The element associated with this Node.  This the key that is used to access this
        // Node in the Dictionary
        public object Element { get; set; }

        // indicates whether this node refers to a view, view model, or view model collection
        public ElementType Type { get; set; }

        // the name that appears in the tree leaf for this node
        public string LeafName { get; set; }

        // the data binding info associated with this node
        public Dictionary<DependencyProperty, BindingExpression> Bindings { get; set; }

        // the parent node of this node
        public Node Parent { get; set; }

        // the children of this Node
        ObservableCollection<Node> _children = new ObservableCollection<Node>();
        public ObservableCollection<Node> Children { get { return _children; } }

        public Node(object element, ElementType type)
        {
            Parent = null;
            Element = element;
            Type = type;
        }
    }
}
