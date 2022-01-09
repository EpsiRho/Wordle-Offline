using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wordle.Classes;

namespace Wordle.ViewModels
{
    public class LetterViewModel
    {
        // UI notification functions
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<Letter> letters = new ObservableCollection<Letter>();
        public ObservableCollection<Letter> Letters
        {
            get { return letters; }
            set
            {
                letters = value;
                this.OnPropertyChanged("Letters");
            }
        }
        public LetterViewModel() { }
    }
}
