using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wordle.Classes
{
    public class Letter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private string character { get; set; }
        public string Character
        {
            get { return character; }
            set
            {
                if (value != character)
                {
                    character = value;
                    this.OnPropertyChanged("Character");
                }
            }
        }
        private SolidColorBrush color { get; set; }
        public SolidColorBrush Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    color = value;
                    this.OnPropertyChanged("Color");
                }
            }
        }
    }
}
