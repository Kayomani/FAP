using System;
using System.Collections.Generic;

namespace ContinuousLinq
{
    // Collection which EXTERNALLY appears to be read-only.
    public class ReadOnlyContinuousCollection<T> : ContinuousCollection<T>
    {
        internal bool IsSealed { get; set; }

        public ReadOnlyContinuousCollection(List<T> list) : base(list)
        {
            IsSealed = true;
        }

        internal ReadOnlyContinuousCollection()
        {
            IsSealed = true;
        }

        protected override void ClearItems()
        {
            if (this.IsSealed)
                throw new NotSupportedException("Collection is read-only.");
            else
                base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            if (this.IsSealed)
                throw new NotSupportedException("Collection is read-only.");
            else
                base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (this.IsSealed)
                throw new NotSupportedException("Collection is read-only.");
            else
                base.MoveItem(oldIndex, newIndex);
        }

        protected override void RemoveItem(int index)
        {
            if (this.IsSealed)
                throw new NotSupportedException("Collection is read-only.");
            else
                base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (this.IsSealed)
                throw new NotSupportedException("Collection is read-only.");
            else
                base.SetItem(index, item);
        }
    }
}
