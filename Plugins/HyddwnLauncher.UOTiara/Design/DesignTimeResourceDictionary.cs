﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HyddwnLauncher.UOTiara.Design
{
    public class DesignTimeResourceDictionary : ResourceDictionary
    {
        private readonly ObservableCollection<ResourceDictionary> _noopMergedDictionaries = new NoopObservableCollection<ResourceDictionary>();

        private class NoopObservableCollection<T> : ObservableCollection<T>
        {
            protected override void InsertItem(int index, T item)
            {
                // Only insert items while in Design Mode (VS is hosting the visualization)
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    base.InsertItem(index, item);
                }
            }
        }

        public DesignTimeResourceDictionary()
        {
            var fieldInfo = typeof(ResourceDictionary).GetField("_mergedDictionaries", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(this, _noopMergedDictionaries);
            }
        }
    }
}
