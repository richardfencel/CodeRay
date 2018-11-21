using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

// Copyright Richard Fencel Software (c) 2014

namespace CodeRayWpfLib
{
    public static class Constants
    {
        public static Color ViewColor = Colors.Red;
        public static Color ViewModelColor = Colors.Blue;
        public static Color TargetObjectColor = Colors.Orchid;
        public static Color ViewModelCollectionColor = Colors.Green;

   
        public static SolidColorBrush ViewBrush = new SolidColorBrush(ViewColor);
        public static SolidColorBrush ViewModelBrush = new SolidColorBrush(ViewModelColor);
        public static SolidColorBrush TargetObjectBrush = new SolidColorBrush(TargetObjectColor);
        public static SolidColorBrush ViewModelCollectionBrush = new SolidColorBrush(ViewModelCollectionColor);
    }
}
