using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;
using EmailClient.Domain;
using System.Waf.Applications;

namespace EmailClient.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExchangeSettingsViewModel : ViewModel<IExchangeSettingsView>
    {
        private readonly ExchangeSettings model;


        [ImportingConstructor]
        public ExchangeSettingsViewModel(IExchangeSettingsView view, ExchangeSettings exchangeSettings) : base(view)
        {
            this.model = exchangeSettings;
        }


        public ExchangeSettings Model { get { return model; } }
    }
}
