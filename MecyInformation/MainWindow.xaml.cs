﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WinSCP;

namespace MecyInformation
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Mesocyclone activeMeso;
        Dictionary<DateTime, List<Mesocyclone>> mesoDict;
        DateTime selectedTime;

        public DateTime SelectedTime { get => selectedTime; set => selectedTime = value; }

        public MainWindow()
        {
            InitializeComponent();

            mesoDict = XMLParser.ParseAllMesos(OpenDataDownloader.LOCAL_DOWNLOAD_PATH);
            lvTimes.ItemsSource = mesoDict;

            gridDetails.DataContext = activeMeso;

            DispatcherTimer clock = new DispatcherTimer();
            clock.Interval = TimeSpan.FromSeconds(1);
            clock.Tick += clockTick;
            clock.Start();
        }

        private void clockTick(object sender, EventArgs e)
        {
            lblClock.Content = "UTC: " + DateTime.UtcNow.ToLongTimeString();
        }

        private void UpdateDetailsPanel()
        {
            activeMeso = (Mesocyclone)lvMesos.SelectedItem;
            gridDetails.DataContext = activeMeso;
        }

        private void lvMesos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDetailsPanel();
        }

        private void lvMesos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvMesos.SelectedItem != null)
            {
                new MapWindow(activeMeso).Show();
            }
        }

        private void lvMesos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult res = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (res.VisualHit.GetType() != typeof(ListBoxItem))
            {
                lvMesos.UnselectAll();
            }
        }

        private void lvTimes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTimes.SelectedItem != null)
            {
                KeyValuePair<DateTime, List<Mesocyclone>> pair = (KeyValuePair<DateTime, List<Mesocyclone>>)lvTimes.SelectedItem;
                DateTime newTime = new DateTime(
                    pair.Key.Year,
                    pair.Key.Month,
                    pair.Key.Day,
                    pair.Key.Hour,
                    pair.Key.Minute,
                    0);
                selectedTime = newTime;
                lvMesos.ItemsSource = mesoDict[selectedTime];
            }
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
    }
}
