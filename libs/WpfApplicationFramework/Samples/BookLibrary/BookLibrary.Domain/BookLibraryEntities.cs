using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Text;
using BookLibrary.Domain.Properties;
using BookLibrary.Foundation;

namespace BookLibrary.Domain
{
    partial class BookLibraryEntities
    {
        public bool HasChanges
        {
            get
            {
                return ObjectStateManager.GetObjectStateEntries(EntityState.Added).Any()
                    || ObjectStateManager.GetObjectStateEntries(EntityState.Modified).Any()
                    || ObjectStateManager.GetObjectStateEntries(EntityState.Deleted).Any();
            }
        }


        partial void OnContextCreated()
        {
            SavingChanges += SavingChangesHandler;
        }

        private void SavingChangesHandler(object sender, EventArgs e)
        {
            StringBuilder errorBuilder = new StringBuilder();
            
            foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(
                EntityState.Added | EntityState.Modified))
            {
                IDataErrorInfo entity = entry.Entity as IDataErrorInfo;
                if (entity != null)
                {
                    string error = entity.Validate();
                    if (!string.IsNullOrEmpty(error))
                    {
                        errorBuilder.AppendInNewLine(string.Format(CultureInfo.CurrentCulture, Resources.EntityInvalid,
                            EntityToString(entity), error));
                    }
                }
            }

            string errorMessage = errorBuilder.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new ValidationException(errorMessage);
            }
        }

        private static string EntityToString(object entity)
        {
            IFormattable formattableEntity = entity as IFormattable;
            if (formattableEntity != null)
            {
                return formattableEntity.ToString(null, CultureInfo.CurrentCulture);
            }
            return entity.ToString();
        }
    }
}
