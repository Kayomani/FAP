using System;
using System.Waf.Applications;
using System.Windows.Input;
using FAP.Application.Views;
using FAP.Domain.Entities;
using Fap.Foundation;

namespace FAP.Application.ViewModel
{
    public class SearchViewModel : ViewModel<ISearchView>
    {
        private bool allowSearch = true;
        private ICommand download;
        private string lowerStatusMessage;
        private DateTime? modifiedDate;
        private string modifiedSearchType;
        private ICommand reset;
        private SafeObservingCollection<SearchResult> results;
        private ICommand search;
        private string searchString;
        private string sizeModifier;
        private string sizeSearchType;
        private double? sizeText;
        private string upperStatusMessage;
        private ICommand viewShare;

        public SearchViewModel(ISearchView v) : base(v)
        {
        }


        public bool AllowSearch
        {
            get { return allowSearch; }
            set
            {
                allowSearch = value;
                RaisePropertyChanged("AllowSearch");
            }
        }

        public string SizeSearchType
        {
            get { return sizeSearchType; }
            set
            {
                sizeSearchType = value;
                RaisePropertyChanged("SizeSearchType");
            }
        }

        public string SizeModifier
        {
            get { return sizeModifier; }
            set
            {
                sizeModifier = value;
                RaisePropertyChanged("SizeModifier");
            }
        }

        public double? SizeText
        {
            get { return sizeText; }
            set
            {
                sizeText = value;
                RaisePropertyChanged("SizeText");
            }
        }

        public string ModifiedSearchType
        {
            get { return modifiedSearchType; }
            set
            {
                modifiedSearchType = value;
                RaisePropertyChanged("ModifiedSearchType");
            }
        }

        public DateTime? ModifiedDate
        {
            get { return modifiedDate; }
            set
            {
                modifiedDate = value;
                RaisePropertyChanged("ModifiedDate");
            }
        }


        public ICommand Reset
        {
            get { return reset; }
            set
            {
                reset = value;
                RaisePropertyChanged("Reset");
            }
        }

        public ICommand Download
        {
            get { return download; }
            set
            {
                download = value;
                RaisePropertyChanged("Download");
            }
        }

        public ICommand ViewShare
        {
            get { return viewShare; }
            set
            {
                viewShare = value;
                RaisePropertyChanged("ViewShare");
            }
        }

        public ICommand Search
        {
            set
            {
                search = value;
                RaisePropertyChanged("Search");
            }
            get { return search; }
        }

        public string SearchString
        {
            set
            {
                searchString = value;
                RaisePropertyChanged("SearchString");
            }
            get { return searchString; }
        }

        public SafeObservingCollection<SearchResult> Results
        {
            get { return results; }
            set
            {
                results = value;
                RaisePropertyChanged("Results");
            }
        }

        public string LowerStatusMessage
        {
            get { return lowerStatusMessage; }
            set
            {
                lowerStatusMessage = value;
                RaisePropertyChanged("LowerStatusMessage");
            }
        }

        public string UpperStatusMessage
        {
            get { return upperStatusMessage; }
            set
            {
                upperStatusMessage = value;
                RaisePropertyChanged("UpperStatusMessage");
            }
        }
    }
}