using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trello {
    public static class Extensions {
        // Returns the values of property (an Expression) as they change,
        // starting with the current value
        public static IObservable<TValue> GetPropertyValues<TSource, TValue>(
            this TSource source, Expression<Func<TSource, TValue>> property)
            where TSource : INotifyPropertyChanged {
            MemberExpression memberExpression = property.Body as MemberExpression;

            if (memberExpression == null) {
                throw new ArgumentException(
                    "property must directly access a property of the source");
            }

            string propertyName = memberExpression.Member.Name;

            Func<TSource, TValue> accessor = property.Compile();

            return source.GetPropertyChangedEvents()
                .Where(x => x.PropertyName == propertyName)
                .Select(x => accessor(source))
                .StartWith(accessor(source));
        }

        // This is a wrapper around FromEvent(PropertyChanged)
        public static IObservable<PropertyChangedEventArgs>
            GetPropertyChangedEvents(this INotifyPropertyChanged source) {
            return Observable.FromEvent<PropertyChangedEventHandler,
                PropertyChangedEventArgs>(
                h => (sender, args) => h(args),
                h => source.PropertyChanged += h,
                h => source.PropertyChanged -= h);
        }

        public static IObservable<TAccumulate> CombinePreviousWithStart<T, TAccumulate>(
            this IObservable<T> observable,
            TAccumulate start, 
            Func<T, T, TAccumulate> mappend) {
            return observable.Scan<T, Tuple<T, TAccumulate>>(Tuple.Create<T, TAccumulate>(default(T), start), (previousTuple, next) => Tuple.Create(next, mappend(previousTuple.Item1, next))).Select(tuple => tuple.Item2);
        }
    }
}
