using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchive.Data.RequestParts;
using WebArchive.Data;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    public class RequestFilterViewModel : NotifyObject
    {
        private readonly RequestFilter BaseFilter;
        public RequestFiltersList ListModel { get; private set; }

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
            set => Set(ref selectedExclude, value);
        }
        private string selectedExclude;

        public ICommand AddExcludeFilterCommand { get; private set; }
        public ICommand RemoveExcludeFilterCommand { get; private set; }
        public ICommand ClearExcludedFiltersCommand { get; private set; }

        public RequestFilterViewModel(RequestFilter baseFilter, RequestFiltersList model)
        {
            ListModel = model;
            BaseFilter = baseFilter;

            AddExcludeFilterCommand = new RelayCommand(OnAddFilterCodeCommandExecuted);
            RemoveExcludeFilterCommand = new RelayCommand(OnRemoveFilterCommandExecuted);
            ClearExcludedFiltersCommand = new RelayCommand(OnClearExcludedFiltersExecuted);
        }

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
            if (exist == null)
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
