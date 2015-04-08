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
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string Title { get; set; }
       
        private bool isChecked;
        public bool IsChecked {
            get { return isChecked; }
            set { isChecked = value; OnPropertyChanged("IsChecked"); }
        }

        public override string ToString() {
            return string.Format("<TodoItem(Title={0}, IsChecked={1})>", Title, IsChecked);
        }

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public ObservableCollection<TodoItem> Todos { get; set; }

        private string header;
        public string Header {
            get { return header; }
            set { header = value; OnPropertyChanged("Header"); }
        }

        public MainViewModel() {
            Todos = new ObservableCollection<TodoItem>();
            Checked.Subscribe(
                n => {
                    Header = String.Format("{0} left", Todos.Count - n);
                });
            
            Todos.Add(new TodoItem { Title = "Eggs" });
            Todos.Add(new TodoItem { Title = "Airplane tickets" });
            Todos.Add(new TodoItem { Title = "Feed husky", IsChecked = true });
        }

        public IObservable<int> Checked {
            get {
                return Observable
                    // Every time the collection changes...
                    .FromEventPattern<NotifyCollectionChangedEventArgs>(Todos, "CollectionChanged")
                    // ... grab all the TodoItems that were added ...
                    .Select(change => change.EventArgs.NewItems.Cast<TodoItem>())
                    // ... plus the current ones ...
                    .StartWith(Todos)
                    // ... flattened into one observable of items instead of an observable of lists of items ...
                    .SelectMany(item => item)
                    // ... map to +1 if the item IsChecked, otherwise 0; then monitor IsChecked and map to +1/-1 accordingly ...
                    .Select(item => item.GetPropertyValues(x => x.IsChecked).CombinePreviousWithStart(0, (bool previous, bool current) => current.CompareTo(previous)))
                    // ... and finally tally up all +1's and -1's ...
                    .Merge().Scan(0, (nChecked, delta) => nChecked + delta);
            }
        }

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            var model = new MainViewModel();
            DataContext = model;
        }
    }
}
