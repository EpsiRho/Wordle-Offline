using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Wordle.Classes;
using Wordle.ViewModels;
using System.Reflection;
using Microsoft.VisualBasic;
using Windows.Storage;
using Newtonsoft.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Wordle
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // Page Vars
        //LetterViewModel RowOneSource { get; set; }
        //LetterViewModel RowTwoSource { get; set; }
        //LetterViewModel RowThreeSource { get; set; }
        //LetterViewModel RowFourSource { get; set; }
        //LetterViewModel RowFiveSource { get; set; }

        List<LetterViewModel> Rows { get; set; }
        int CurrentRow { get; set; }
        int CurrentLetter { get; set; }
        string CurrentWord { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();
            Rows = new List<LetterViewModel>();
            CurrentRow = 0;
            CurrentWord = "";
            CurrentLetter = 0;
            for (int i = 0; i < 7; i++)
            {
                Rows.Add(new LetterViewModel());
            }
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Rows[i].Letters.Add(new Letter() { Character = "", Color = new SolidColorBrush(Color.FromArgb(255, 18, 18, 19)) });
                }
            }
            Thread t = new Thread(GetWord);
            t.Start();

            Thread d = new Thread(LoadStatsFile);
            d.Start();

        }
        
        private void GetWord()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("sgb-words.txt"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                List<string> words = result.Split('\n').ToList();

                Random rand = new Random();
                int index = rand.Next(words.Count);

                CurrentWord = words[index].ToUpper();
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    CurrentWordDisplay.Text = CurrentWord;
                });
            }
        }
        
        private async void LoadStatsFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string[] dirs = Directory.GetDirectories(path);
            if (!dirs.Contains("Wordle"))
            {
                System.IO.Directory.CreateDirectory($"{path}\\Wordle");
            }

            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync($"{path}\\Wordle");
            StorageFile statsFile = null;
            try
            {
                statsFile = await storageFolder.GetFileAsync($"stats.stt");
            }
            catch (Exception)
            {
                statsFile = await storageFolder.CreateFileAsync($"stats.stt");
            }
            string txt = await Windows.Storage.FileIO.ReadTextAsync(statsFile, Windows.Storage.Streams.UnicodeEncoding.Utf8);

            var split = txt.Split(";");

            if(split.Length > 1)
            {
                UserStats.Played = Convert.ToUInt16(split[0]);
                UserStats.WinNum = Convert.ToUInt16(split[1]);
                UserStats.CurrentStreak = Convert.ToUInt16(split[2]);
                UserStats.MaxStreak = Convert.ToUInt16(split[3]);
                UserStats.distro = new List<double>();
                var dis = split[4].Split("|");
                foreach(var d in dis)
                {
                    UserStats.distro.Add(Convert.ToUInt16(d));
                }
            }
            else
            {
                await Windows.Storage.FileIO.WriteTextAsync(statsFile, "0;0;0;0;0|0|0|0|0|0");
            }
        }

        private async void CheckWord()
        {
            GridView row = null;
            switch (CurrentRow)
            {
                case 0:
                    row = RowOneGrid;
                    break;
                case 1:
                    row = RowTwoGrid;
                    break;
                case 2:
                    row = RowThreeGrid;
                    break;
                case 3:
                    row = RowFourGrid;
                    break;
                case 4:
                    row = RowFiveGrid;
                    break;
                case 5:
                    row = RowSixGrid;
                    break;
                default:
                    row = RowSixGrid;
                    break;
            }
            int correctCount = 0;

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("sgb-words.txt"));
            List<string> words = new List<string>();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                words = result.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            }
           
            string wrd = $"{Rows[CurrentRow].Letters[0].Character}{Rows[CurrentRow].Letters[1].Character}{Rows[CurrentRow].Letters[2].Character}{Rows[CurrentRow].Letters[3].Character}{Rows[CurrentRow].Letters[4].Character}";

            if (!words.Contains(wrd.ToLower()))
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    WordError.Visibility = Visibility.Visible;
                });
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                string cc = Rows[CurrentRow].Letters[i].Character;
                if (cc == CurrentWord[i].ToString())
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        Rows[CurrentRow].Letters[i].Color = new SolidColorBrush(Color.FromArgb(255, 83, 141, 78));
                    });
                    correctCount++;
                }
                else if (CurrentWord.Contains(cc))
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        Rows[CurrentRow].Letters[i].Color = new SolidColorBrush(Color.FromArgb(255, 181, 159, 59));

                    });
                }
                else
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        Rows[CurrentRow].Letters[i].Color = new SolidColorBrush(Color.FromArgb(255, 58, 58, 60));

                    });
                }

                Thread.Sleep(300);

            }

            if(correctCount == 5)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    UserStats.CurrentStreak++;
                    UserStats.Played++;
                    UserStats.WinNum++;
                    if(UserStats.CurrentStreak > UserStats.MaxStreak)
                    {
                        UserStats.MaxStreak++;
                    }

                    double total = 0;
                    foreach (var i in UserStats.distro)
                    {
                        total += i;
                    }

                    total++;
                    for (int i = 0; i < 6 ; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                if(CurrentRow == 0)
                                {
                                    UserStats.distro[i]++;
                                }
                                GDTextOne.Text = UserStats.distro[i].ToString();
                                GDProgressOne.Value = (UserStats.distro[i] / total) * 100;
                                break;
                            case 1:
                                if (CurrentRow == 1)
                                {
                                    UserStats.distro[i]++;
                                }
                                GDTextTwo.Text = UserStats.distro[i].ToString();
                                GDProgressTwo.Value = (UserStats.distro[i] / total) * 100;
                                break;
                            case 2:
                                if (CurrentRow == 2)
                                {
                                    UserStats.distro[i]++;
                                }
                                GDTextThree.Text = UserStats.distro[i].ToString();
                                GDProgressThree.Value = (UserStats.distro[i] / total) * 100;
                                break;
                            case 3:
                                if (CurrentRow == 3)
                                {
                                    UserStats.distro[i]++;
                                }
                                GDTextFour.Text = UserStats.distro[i].ToString();
                                GDProgressFour.Value = (UserStats.distro[i] / total) * 100;
                                break;
                            case 4:
                                if (CurrentRow == 4)
                                {
                                    UserStats.distro[i]++;
                                }
                                GDTextFive.Text = UserStats.distro[i].ToString();
                                GDProgressFive.Value = (UserStats.distro[i] / total) * 100;
                                break;
                            case 5:
                                if (CurrentRow == 5)
                                {
                                    UserStats.distro[i]++;
                                }
                                GDTextSix.Text = UserStats.distro[i].ToString();
                                GDProgressSix.Value = (UserStats.distro[i] / total) * 100;
                                break;

                        }
                    }
                    PlayedStatText.Text = UserStats.Played.ToString();
                    double x = ((UserStats.WinNum / UserStats.Played) * 100);
                    WinPercentText.Text = String.Format("{0:0.##}", x);
                    CurrentStreakText.Text = UserStats.CurrentStreak.ToString();
                    MaxStreakText.Text = UserStats.MaxStreak.ToString();
                    WinPopup.Visibility = Visibility.Visible;
                });
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync($"{path}\\Wordle");
                StorageFile statsFile = null;
                try
                {
                    statsFile = await storageFolder.GetFileAsync($"stats.stt");
                }
                catch (Exception)
                {
                    return;
                }
                await Windows.Storage.FileIO.WriteTextAsync(statsFile, $"{UserStats.Played};{UserStats.WinNum};{UserStats.CurrentStreak};{UserStats.MaxStreak};{UserStats.distro[0]}|{UserStats.distro[1]}|{UserStats.distro[2]}|{UserStats.distro[3]}|{UserStats.distro[4]}|{UserStats.distro[5]}");
                return;
            }

            if(CurrentRow == 5 && correctCount != 5)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    UserStats.CurrentStreak = 0;
                    UserStats.Played++;
                    UserStats.MaxStreak = 0;
                    PlayedStatText.Text = UserStats.Played.ToString();
                    double x = ((UserStats.WinNum / UserStats.Played) * 100);
                    WinPercentText.Text = String.Format("{0:0.##}", x);
                    CurrentStreakText.Text = UserStats.CurrentStreak.ToString();
                    MaxStreakText.Text = UserStats.MaxStreak.ToString();
                    WinPopup.Visibility = Visibility.Visible;
                });
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync($"{path}\\Wordle");
                StorageFile statsFile = null;
                try
                {
                    statsFile = await storageFolder.GetFileAsync($"stats.stt");
                }
                catch (Exception)
                {
                    return;
                }
                await Windows.Storage.FileIO.WriteTextAsync(statsFile, $"{UserStats.Played};{UserStats.WinNum};{UserStats.CurrentStreak};{UserStats.MaxStreak};{UserStats.distro[0]}|{UserStats.distro[1]}|{UserStats.distro[2]}|{UserStats.distro[3]}|{UserStats.distro[4]}|{UserStats.distro[5]}");
                return;
            }



            this.DispatcherQueue.TryEnqueue(() =>
            {
                row.SelectedIndex = -1;
                CurrentRow++;
                CurrentLetter = 0;
                switch (CurrentRow)
                {
                    case 0:
                        row = RowOneGrid;
                        break;
                    case 1:
                        row = RowTwoGrid;
                        break;
                    case 2:
                        row = RowThreeGrid;
                        break;
                    case 3:
                        row = RowFourGrid;
                        break;
                    case 4:
                        row = RowFiveGrid;
                        break;
                    case 5:
                        row = RowSixGrid;
                        break;
                    default:
                        row = RowSixGrid;
                        break;
                }
                row.SelectedIndex = 0;
            });
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            GridView row = null;
            switch (CurrentRow)
            {
                case 0:
                    row = RowOneGrid;
                    break;
                case 1:
                    row = RowTwoGrid;
                    break;
                case 2:
                    row = RowThreeGrid;
                    break;
                case 3:
                    row = RowFourGrid;
                    break;
                case 4:
                    row = RowFiveGrid;
                    break;
                case 5:
                    row = RowSixGrid;
                    break;
                default:
                    row = RowSixGrid;
                    break;
            }
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                if (CurrentRow != 6)
                {
                    if (CurrentLetter == 5)
                    {
                        Thread t = new Thread(CheckWord);
                        t.Start();
                    }
                }
            }
            else if(e.Key == Windows.System.VirtualKey.Back)
            {
                if(CurrentLetter == 0)
                { 
                    if(Rows[CurrentRow].Letters[CurrentLetter].Character != "")
                    {
                        Rows[CurrentRow].Letters[CurrentLetter].Character = "";
                    }
                }
                else
                {
                    CurrentLetter--;
                    row.SelectedIndex = CurrentLetter;
                    Rows[CurrentRow].Letters[CurrentLetter].Character = "";
                }
                WordError.Visibility = Visibility.Collapsed;
            }
            else
            {
                if(e.Key.ToString().Length == 1)
                {
                    if (CurrentLetter == 5 || row.SelectedIndex == -1)
                    {
                        return;
                    }
                    Rows[CurrentRow].Letters[CurrentLetter].Character = e.Key.ToString();
                    if (CurrentLetter != 5)
                    {
                        CurrentLetter++;
                        if(CurrentLetter == 5)
                        {
                            row.SelectedIndex = -1;
                            return;
                        }
                        row.SelectedIndex = CurrentLetter;
                        WordError.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentRow = 0;
            CurrentWord = "";
            CurrentLetter = 0;
            RowOneGrid.SelectedIndex = 0;
            RowTwoGrid.SelectedIndex = -1;
            RowThreeGrid.SelectedIndex = -1;
            RowFourGrid.SelectedIndex = -1;
            RowFiveGrid.SelectedIndex = -1;
            RowSixGrid.SelectedIndex = -1;
            RowSixGrid.SelectedIndex = -1;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Rows[i].Letters[j].Character = "";
                    Rows[i].Letters[j].Color = new SolidColorBrush(Color.FromArgb(255, 18, 18, 19));
                }
            }
            Thread t = new Thread(GetWord);
            t.Start();
            WinPopup.Visibility = Visibility.Collapsed;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPane.Visibility = Visibility.Visible;
        }

        private void SettingsCloseButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPane.Visibility = Visibility.Collapsed;
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
