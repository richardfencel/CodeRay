using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

// Copyright Richard Fencel Software (c) 2014

namespace CodeRayWpfLib
{
    public class BindingRecord
    {
        // take from BindingExpression
        public string SourceViewModel{ get; set; }
        public object SourceViewModelObj { get; set; }
        public string SourceProperty { get; set; }
        public string TargetView { get; set; }
        public object TargetViewObj { get; set; }
        public string TargetObject { get; set; }
        public object TargetObjectObj { get; set; }
        public string TargetProperty {get; set;}
        public string Value { get; set; }
        public Update UpdateDirection { get; set; }
        public BindingMode Mode { get; set; }

        // taken from BindingBase
        public string BindingGroupName {get; set;}
        public string Delay { get; set; }
        public string FallbackValue { get; set; }
        public string Format { get; set; }
        public string TargetNullValue { get; set; }
    }
}
