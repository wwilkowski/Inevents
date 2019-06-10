﻿using Inevent.Elements;
using Inevent.Models;
using Inevent.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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

namespace Inevent.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public int UserID { get; set; }
        public Event[] signedEvents { get; set; }
        public DashboardView()
        {
            InitializeComponent();
            LoadEvents();
            UserID = Properties.Settings.Default.id;
        }

        
        private bool ifSigned(int a, int[] b)
        {
            for (int i=0; i<b.Length; i++)
            {
                if (b[i] == a) return true;
            }
            return false;
        }

        private async void LoadEvents()
        {
            try
            {
                Event[] upcomingEvents = await Events.LoadEvents();
                foreach (Event ev in upcomingEvents)
                {
                    ev.FormatedDate = ev.Date.ToString("dddd, dd MMMM yyyy HH:mm");
                    ev.FormatedDay = ev.Date.ToString("dd");
                    ev.FormatedDayName = ev.Date.ToString("dddd").Substring(0, 3).ToUpper();
                    TimeSpan t = ev.Date - DateTime.Now;
                    if (ev.Date > DateTime.Now)
                    {
                        ev.Countdown = string.Format("{0} dni, {1} godzin, {2} minut, {3} sekund", t.Days, t.Hours, t.Minutes, t.Seconds);
                    }
                    else
                    {
                        upcomingEvents = upcomingEvents.Where(val => val.Id != ev.Id).ToArray();
                        ev.Countdown = "Wydarzenie odbyło się.";
                    }



                }
                if (upcomingEvents.Length > 0)
                {
                    Upcoming.ItemsSource = upcomingEvents;
                }
                else
                {
                    IfEmpty.Text = "Brak nadchodzących wydarzeń";
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        void JoinButton_click(object sender, EventArgs e)
        {
            Properties.Settings.Default.currentEvent = Convert.ToInt32((sender as Button).Tag);
            Content = new EventInfoModel();
        }

        void EventTile_click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Properties.Settings.Default.currentEvent = Convert.ToInt32(btn.CommandParameter);
            Content = new EventInfoModel();
        }

        void DashboardButton_click(object sender, EventArgs e)
        {
            Content = new DashboardModel();
        }

        void Test_click(object sender, EventArgs e)
        {
            MessageBox.Show("JEST");
        }
    }
}
