using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeseumClient.ViewModels
{
    public class ExhibitsViewModel : BaseViewModel
    {
        public ObservableCollection<string> Exhibits { get; set; }

        public ExhibitsViewModel()
        {
            Exhibits = new ObservableCollection<string>
        {
            "Картина Мона Лиза",
            "Скульптура Давид",
            "Древний амулет"
        };
        }
    }
}
