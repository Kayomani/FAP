using System;
using System.Collections.ObjectModel;
using System.Text;
using Fap.Foundation;
using Newtonsoft.Json;

namespace FAP.Domain.Entities.FileSystem
{
    public class BrowsingFile : BaseEntity
    {
        private bool populated;
        private ObservableCollection<BrowsingFile> subItems = new ObservableCollection<BrowsingFile>();
        private BrowsingFile temp;

        public ObservableCollection<BrowsingFile> Items
        {
            set { subItems = value; }
            get { return subItems; }
        }


        [JsonIgnore]
        public FilteredObservableCollection<BrowsingFile> Folders
        {
            get
            {
                if (!IsPopulated && subItems.Count == 0)
                {
                    temp = new BrowsingFile {IsFolder = true};
                    subItems.Add(temp);
                }
                var lcv = new FilteredObservableCollection<BrowsingFile>(subItems);
                lcv.Filter = i => (i).IsFolder;
                return lcv;
            }
        }

        public bool IsPopulated
        {
            set
            {
                populated = value;
                if (value)
                {
                    if (subItems.Contains(temp))
                        subItems.Remove(temp);
                }
            }
            get { return populated; }
        }

        public bool IsFolder { set; get; }
        public string Name { set; get; }
        public long Size { set; get; }
        public DateTime LastModified { set; get; }

        public string Extension
        {
            get
            {
                if (null == Name)
                    return string.Empty;
                return System.IO.Path.GetExtension(Name);
            }
        }

        [JsonIgnore]
        public string FullPath
        {
            get
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(Path))
                {
                    sb.Append(Path);
                    sb.Append("/");
                }
                sb.Append(Name);
                return sb.ToString();
            }
            set
            {
                if (value.Contains("/"))
                {
                    int split = value.LastIndexOf("/");
                    Path = value.Substring(0, split);
                    Name = value.Substring(split + 1, value.Length - (split + 1));
                }
                else
                {
                    Name = value;
                }
            }
        }

        public string Path { set; get; }

        public void AddItem(BrowsingFile ent)
        {
            subItems.Add(ent);
        }

        public void ClearItems()
        {
            if (subItems.Count > 0)
                subItems.Clear();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}