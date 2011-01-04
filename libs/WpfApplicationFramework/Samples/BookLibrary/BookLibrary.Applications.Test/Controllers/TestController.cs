using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using BookLibrary.Applications.Controllers;
using BookLibrary.Applications.Test.Services;
using BookLibrary.Applications.Test.Views;
using BookLibrary.Applications.ViewModels;

namespace BookLibrary.Applications.Test.Controllers
{
    public class TestController
    {
        private readonly CompositionContainer container;
        

        public TestController()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new TypeCatalog(
                typeof(ApplicationController), typeof(BookController), typeof(PersonController), typeof(EntityControllerMock),
                typeof(PresentationControllerMock),
                typeof(ShellViewModel), typeof(BookViewModel), typeof(PersonViewModel)
            ));
            catalog.Catalogs.Add(new TypeCatalog(
                typeof(QuestionServiceMock), typeof(MessageServiceMock), typeof(EntityServiceMock),
                typeof(ShellViewMock), typeof(BookListViewMock), typeof(BookViewMock), typeof(PersonListViewMock), 
                typeof(PersonViewMock), typeof(LendToViewMock)
            ));
            container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);
        }


        public CompositionContainer Container { get { return container; } }
    }
}
