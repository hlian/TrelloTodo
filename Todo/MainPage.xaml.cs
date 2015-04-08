using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Trello {
    public class TodoItem : INotifyPropertyChanged {
        public string Title { get; set; }
       
        private bool isChecked;
        public bool IsChecked {
            get { return isChecked; }
            set { isChecked = value; OnPropertyChanged("IsChecked"); }
        }

        public override string ToString() {
            return string.Format("<TodoItem(Title={0}, IsChecked={1})>", Title, IsChecked);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string propertyName) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed partial class MainPage : Page {
        public ObservableCollection<TodoItem> Todos = new ObservableCollection<TodoItem>();

        public MainPage() {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            Observable
                // Every time the collection changes...
                .FromEventPattern<NotifyCollectionChangedEventArgs>(Todos, "CollectionChanged")
                // ... grab all the TodoItems that were added ...
                .SelectMany(change => change.EventArgs.NewItems.Cast<TodoItem>())
                // ... map to +1 if the item IsChecked, otherwise 0; then monitor IsChecked and map to +1/-1 accordingly ...
                .Select(item => item.GetPropertyValues(x => x.IsChecked).CombinePreviousWithStart(0, (bool previous, bool current) => current.CompareTo(previous)))
                // ... and finally tally up all +1's and -1's ...
                .Merge().Scan(0, (nChecked, delta) => nChecked + delta)
                // ... so we can print it out!
                .Subscribe(nChecked => Debug.WriteLine("Number of checked items: {0}", nChecked));

            Todos.Add(new TodoItem { Title = "Eggs" });
            Todos.Add(new TodoItem { Title = "Airplane tickets" });
            Todos.Add(new TodoItem { Title = "Feed husky", IsChecked = true });

            ListView.DataContext = this.Todos;
        }
    }
}
