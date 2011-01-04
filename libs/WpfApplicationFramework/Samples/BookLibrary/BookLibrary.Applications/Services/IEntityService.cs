using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Domain;
using System.Collections.ObjectModel;

namespace BookLibrary.Applications.Services
{
    public interface IEntityService
    {
        ObservableCollection<Book> Books { get; }

        ObservableCollection<Person> Persons { get; }
    }
}
