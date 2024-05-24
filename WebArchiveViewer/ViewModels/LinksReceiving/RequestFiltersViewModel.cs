using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using WebArchive.Data;
using WebArchive.Data.RequestParts;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    /// <summary>
    /// Визуальный редактор типов контента и статус-кодов
    /// </summary>
    public class RequestFiltersViewModel : NotifyObject
    {
        private readonly ArchiveRequestBuilder RequestBuilder;

        public RequestFilterViewModel TypesView { get; private set; }
        public RequestFilterViewModel CodesView { get; private set; }

        public RequestFiltersList CodesList => RequestBuilder.Codes.FilterConstructor;
        public RequestFiltersList TypesList => RequestBuilder.Types.FilterConstructor;

        public IEnumerable<string> CodesSources => CodesList.GetSourceWithoutPicked();
        public IEnumerable<string> TypesSources => TypesList.GetSourceWithoutPicked();

        public bool CodesSourceIsNotEmpty => CodesSources.Any();
        public bool TypesSourceIsNotEmpty => TypesSources.Any();

        public string SelectedCode
        {
            get => selectedCode;
            set => Set(ref selectedCode, value);
        }
        private string selectedCode;
        public string SelectedType
        {
            get => selectedType;
            set => Set(ref selectedType, value);
        }
        private string selectedType;

        public ICommand AddFilterCodeCommand { get; private set; }
        public ICommand AddFilterTypeCommand { get; private set; }
        public ICommand RemoveFilterCommand { get; private set; }


        public RequestFiltersViewModel(ArchiveRequestBuilder builder)
        {
            RequestBuilder = builder;

            CodesList.SetCollection(new ObservableCollection<RequestFilterItem>());
            TypesList.SetCollection(new ObservableCollection<RequestFilterItem>());

            TypesView = new RequestFilterViewModel(builder.Types, TypesList);
            CodesView = new RequestFilterViewModel(builder.Codes, CodesList);

            AddFilterCodeCommand = new RelayCommand(OnAddFilterCodeCommandExecuted);
            AddFilterTypeCommand = new RelayCommand(OnAddFilterTypeCommandExecuted);
            RemoveFilterCommand = new RelayCommand(OnRemoveFilterCommandExecuted);
        }



        private void OnAddFilterCodeCommandExecuted(object o)
        {
            AddSelectedFilterToList(CodesList, SelectedCode, nameof(CodesSources), nameof(CodesSourceIsNotEmpty));
            SelectedCode = CodesSources.FirstOrDefault();
        }
        private void OnAddFilterTypeCommandExecuted(object o)
        {
            AddSelectedFilterToList(TypesList, SelectedType, nameof(TypesSources), nameof(TypesSourceIsNotEmpty));
            SelectedType = TypesSources.FirstOrDefault();
        }
        private void AddSelectedFilterToList(RequestFiltersList list, string selected, params string[] propertyNames)
        {
            var exist = list.List.FirstOrDefault(t => t.Content == selected);
            if (exist != null || string.IsNullOrEmpty(selected))
            {
                return;
            }
            list.List.Add(new RequestFilterItem() { Content = selected, Enabled = true });
            foreach (var name in propertyNames)
            {
                OnPropertyChanged(name);
            }
        }


        private void OnRemoveFilterCommandExecuted(object o)
        {
            if (!(o is RequestFilterItem codeType))
            {
                return;
            }
            var lists = new (IList<RequestFilterItem> list, string propertyName, string propertyName2)[]
            {
                (RequestBuilder.Types.FilterConstructor.List, nameof(TypesSources), nameof(TypesSourceIsNotEmpty)),
                (RequestBuilder.Codes.FilterConstructor.List, nameof(CodesSources), nameof(CodesSourceIsNotEmpty))
            };

            foreach (var list in lists)
            {
                var exist = list.list.FirstOrDefault(t => t.Content == codeType.Content);
                if (exist != null)
                {
                    list.list.Remove(exist);
                    OnPropertyChanged(list.propertyName);
                    OnPropertyChanged(list.propertyName2);
                }
            }

        }
    }
}
