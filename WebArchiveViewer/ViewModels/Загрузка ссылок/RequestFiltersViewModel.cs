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
    public class RequestFilterViewModel : NotifyObject
    {
        private readonly RequestFilter BaseFilter;
        public RequestFiltersList ListModel { get; private set; }

        public RequestFilterViewModel(RequestFilter baseFilter, RequestFiltersList model)
        {
            ListModel = model;
            BaseFilter = baseFilter;

            AddExcludeFilterCommand = new RelayCommand(OnAddFilterCodeCommandExecuted);
            RemoveExcludeFilterCommand = new RelayCommand(OnRemoveFilterCommandExecuted);
            ClearExcludedFiltersCommand = new RelayCommand(OnClearExcludedFiltersExecuted);
        }

        public int Mode
        {
            get => mode;
            set
            {
                Set(ref mode, value);
                switch (mode)
                {
                    case 0:
                        BaseFilter.Enabled = false;
                        break;
                    case 1:
                        BaseFilter.Enabled = true;
                        ListModel.SetSelected(Selected);
                        break;
                    case 2:
                        BaseFilter.Enabled = true;
                        ListModel.SetSelected("");
                        break;
                }
            }
        }
        private int mode;

        public string Selected
        {
            get => selected;
            set
            {
                Set(ref selected, value);
                ListModel.SetSelected(value);
            }
        }
        private string selected;

        public IEnumerable<string> SourcesForExcluded => ListModel.GetSourceWithoutPicked();
        public bool SourceIsNotEmpty => SourcesForExcluded.Any();


        public string SelectedExclude
        {
            get => selectedExclude;
            set=> Set(ref selectedExclude, value);
        }
        private string selectedExclude;

        public ICommand AddExcludeFilterCommand { get; private set; }
        public ICommand RemoveExcludeFilterCommand { get; private set; }
        public ICommand ClearExcludedFiltersCommand { get; private set; }


        private void OnAddFilterCodeCommandExecuted(object o)
        {
            var exist = ListModel.List.FirstOrDefault(t => t.Content == SelectedExclude);
            if (exist != null || string.IsNullOrEmpty(SelectedExclude))
            {
                return;
            }
            ListModel.List.Add(new RequestFilterItem() { Content = SelectedExclude, Enabled = false });
            SelectedExclude = SourcesForExcluded.FirstOrDefault();
            OnPropertyChanged(nameof(SourcesForExcluded));
            OnPropertyChanged(nameof(SourceIsNotEmpty));
        }
        private void OnRemoveFilterCommandExecuted(object o)
        {
            if (!(o is RequestFilterItem codeType))
            {
                return;
            }

            var exist = ListModel.List.FirstOrDefault(t => t.Content == codeType.Content);
            if(exist == null)
            {
                return;
            }
            ListModel.List.Remove(exist);
            OnPropertyChanged(nameof(SourcesForExcluded));
            OnPropertyChanged(nameof(SourceIsNotEmpty));
        }
        private void OnClearExcludedFiltersExecuted(object o)
        {

        }
    }
}
