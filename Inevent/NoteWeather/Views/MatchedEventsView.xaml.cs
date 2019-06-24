﻿using Inevent.Models;
using Inevent.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MatchedEventsView.xaml
    /// </summary>
    public partial class MatchedEventsView : UserControl
    {
        public List<Event> matchedEvents { get; set; }
        public Tag[] userTags { get; set; }
        ObservableCollection<Event> observableEvents { get; set; }

        public MatchedEventsView()
        {
            InitializeComponent();
            LoadUserTags();
        }

        private async void LoadUserTags()
        {
            userTags = await Tags.GetUserTags(Properties.Settings.Default.id);
            LoadEvents();
        }

        private double CompareTags(string[] tags)
        {
            int i = 0;
            if (tags.Length == 0)
            {
                return 0.0;
            }
            foreach(Tag t in userTags)
            {
                if (tags.Contains(t.Value))
                {
                    i++;
                }
            }
            double result = ((double)i / (double)tags.Length) * 100;
            return Math.Round(result,0);
        }

        private async void LoadEvents()
        {
            matchedEvents = new List<Event>();
            try
            {
                int[] eventsIds = await Events.LoadEventsId();

                foreach (int eventId in eventsIds)
                {
                    Event[] newEvent = await Events.LoadEvent(eventId);
                    newEvent[0].MatchedValue = CompareTags(newEvent[0].Tags);
                    newEvent[0].FormatedDate = newEvent[0].Date.ToString("dddd, dd MMMM yyyy HH:mm");
                    newEvent[0].FormatedDay = newEvent[0].Date.ToString("dd");
                    newEvent[0].FormatedDayName = newEvent[0].Date.ToString("dddd").Substring(0, 3).ToUpper();
                    TimeSpan t = newEvent[0].Date - DateTime.Now;
                    if (newEvent[0].Date > DateTime.Now)
                    {
                        newEvent[0].Countdown = string.Format("{0} dni, {1} godzin, {2} minut, {3} sekund", t.Days, t.Hours, t.Minutes, t.Seconds);
                        matchedEvents.Add(newEvent[0]);
                    }
                }
                Event[] result = matchedEvents.ToArray();
                observableEvents = new ObservableCollection<Event>(result.OrderBy(p => p.Date));
                if (result.Length > 0)
                {
                    result.OrderBy(p => p.MatchedValue);
                    Matched.ItemsSource = observableEvents;
                    Thread timeUpdater = new Thread(() =>
                    {
                        CancellationToken token = new CancellationToken();
                        for (; ; )
                        {
                            if (!token.IsCancellationRequested)
                            {
                                foreach (Event ev in observableEvents)
                                {
                                    TimeSpan t = ev.Date - DateTime.Now;
                                    if (ev.Date > DateTime.Now)
                                    {
                                        ev.Countdown = string.Format("{0} dni, {1} godzin, {2} minut, {3} sekund", t.Days, t.Hours, t.Minutes, t.Seconds);
                                    }
                                    else
                                    {
                                        ev.Countdown = "Wydarzenie odbyło się.";
                                    }
                                }
                                Task.Delay(1000, token).Wait(); // use await for async method

                            }
                            else
                            {
                                break; // end
                            }
                        }
                    });
                    timeUpdater.IsBackground = true;
                    timeUpdater.Start();
                }
                else
                {
                    IfEmpty.Text = "Nie dopasowano żadnego wydarzenia.";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void DetailsButton_click(object sender, EventArgs e)
        {
            Properties.Settings.Default.currentEvent = Convert.ToInt32((sender as Button).Tag);
            Content = new EventInfoModel();
        }
    }
}
