using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using ViewModel;
using PropertyChanged;

namespace ExpenseWatcher.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ReadCsvPageVM: BaseViewModel
    {
        private List<string> allLines;

        const char DefaultSeperator = ';';

        public ReadCsvPageVM()
        {
            Payments = Globals.Payments;
            ReadCsvPartCommand = new RelayCommand(ReadCsvPart);
        }
                
        public ObservableCollection<Payment> Payments { get; set; }

        public ObservableCollection<string> Columns { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<ObservableCollection<string>> Rows { get; set; } = new ObservableCollection<ObservableCollection<string>>();    

        public string FirstLines { get; private set; }

        #region event handlers

        #endregion

        #region Commands
        public RelayCommand ReadCsvPartCommand { get; private set; }

        private void ReadCsvPart(object o)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            if (ofd.ShowDialog() == true)
            {
                FirstLines = String.Empty;
                allLines = File.ReadAllLines(ofd.FileName).ToList();
                for (int i= 0;i<allLines.Count;i++)
                {
                    FirstLines += allLines[i] + "\r\n";
                    if (i>=3)
                    {
                        break;
                    }
                }

                Columns.Clear();
                Rows.Clear();
                allLines[0].Split(DefaultSeperator).ToList().ForEach(s => Columns.Add(s)); Rows.Add(new ObservableCollection<string>()) ;

                foreach (var line in allLines.Skip(1))
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    try
                    {
                        int i = 0;
                        var splits=line.Split(DefaultSeperator).ToList();
                        foreach (var s in splits)
                        {
                            Rows.ElementAt(i).Add(s);
                            i++;
                        }                        
                    }
                    catch (Exception ex)
                    {
                        Logging.Log.Error($"Could not parse {line}. {ex}");
                        continue;
                    }
                }
            }
        }
        #endregion
    }
}
