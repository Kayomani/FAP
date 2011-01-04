using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Data.Objects;

namespace BookLibrary.Applications.Services
{
    internal class EntityObservableCollection<T> : ObservableCollection<T>
    {
        private readonly ObjectContext objectContext;
        private readonly string entitySetName;

        
        public EntityObservableCollection(ObjectContext objectContext, string entitySetName, IEnumerable<T> items) 
            : base(items ?? new T[] {})
        {
            if (objectContext == null) { throw new ArgumentNullException("objectContext"); }
            if (entitySetName == null) { throw new ArgumentNullException("entitySetName"); }

            this.objectContext = objectContext;
            this.entitySetName = entitySetName;
        }
        

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            objectContext.AddObject(entitySetName, item);
        }

        protected override void RemoveItem(int index)
        {
            T itemToDelete = this[index];
            base.RemoveItem(index);

            objectContext.DeleteObject(itemToDelete);
        }

        protected override void ClearItems()
        {
            T[] itemsToDelete = this.ToArray<T>();
            base.ClearItems();

            foreach (T item in itemsToDelete)
            {
                objectContext.DeleteObject(item);
            }
        }

        protected override void SetItem(int index, T item)
        {
            T itemToReplace = this[index];
            base.SetItem(index, item);

            objectContext.DeleteObject(itemToReplace);
            objectContext.AddObject(entitySetName, item);
        }
    }
}
