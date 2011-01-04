using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using BookLibrary.Domain;
using System.Collections.Generic;
using System.Linq;

namespace BookLibrary.Applications.Services
{
    [Export(typeof(IEntityService)), Export]
    public class EntityService : IEntityService
    {
        private BookLibraryEntities entities;
        private EntityObservableCollection<Book> books;
        private EntityObservableCollection<Person> persons;


        public BookLibraryEntities Entities
        {
            get { return entities; }
            set { entities = value; }
        }

        public ObservableCollection<Book> Books
        {
            get 
            {
                if (books == null && entities != null)
                {
                    IQueryable<Book> booksQuery = entities.CreateQuery<Book>("[Books]").Include("LendTo");
                    books = new EntityObservableCollection<Book>(entities, "Books", booksQuery);
                }
                return books;
            }
        }

        public ObservableCollection<Person> Persons
        {
            get 
            {
                if (persons == null && entities != null)
                {
                    IQueryable<Person> personsQuery = entities.CreateQuery<Person>("[Persons]").Include("Borrowed");
                    persons = new EntityObservableCollection<Person>(entities, "Persons", personsQuery);
                }
                return persons;
            }
        }
    }
}
