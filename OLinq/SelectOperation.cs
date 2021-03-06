﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SelectOperation<TSource, TResult> : EnumerableSourceWithLambdaOperation<TSource, TResult, IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        public SelectOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, TResult>(1))
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnLambdaCollectionItemsAdded(IEnumerable<LambdaOperation<TResult>> newItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.Select(i => i.Value).ToList(), startingIndex));
        }

        protected override void OnLambdaCollectionItemsRemoved(IEnumerable<LambdaOperation<TResult>> oldItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.Select(i => i.Value).ToList(), startingIndex));
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, TResult> args)
        {
            var oldValue = (TResult)args.OldValue;
            var newValue = (TResult)args.NewValue;

            // single value has been replaced
            if (!object.Equals(oldValue, newValue))
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new TResult[] { newValue }, new TResult[] { oldValue }));
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return Lambdas.Select(i => i.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
