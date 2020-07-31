using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Controls;

namespace ExpanseWatcher
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            this.DataContext = new ViewModel();
            //MailRepository.ReadImap();
        }

        

    }

    public class ViewModel
    {
        public Page DisplayPage { get; set; }

        public ViewModel ()
        {
            DisplayPage = new Views.NameReplacements();
        }

    }
}
