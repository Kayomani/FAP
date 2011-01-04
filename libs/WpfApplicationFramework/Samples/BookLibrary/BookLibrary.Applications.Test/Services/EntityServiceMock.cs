using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Services;
using System.Collections.ObjectModel;
using BookLibrary.Domain;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Services
{
    [Export(typeof(IEntityService))]
    public class EntityServiceMock : IEntityService
    {
        public EntityServiceMock()
        {
            Books = new ObservableCollection<Book>();
            Persons = new ObservableCollection<Person>();
        }


        public ObservableCollection<Book> Books { get; private set; }

        public ObservableCollection<Person> Persons { get; private set; }
    }
}
