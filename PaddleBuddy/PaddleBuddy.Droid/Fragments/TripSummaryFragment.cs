using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Droid.Fragments
{
    public class TripSummaryFragment : BaseFragment
    {
        private EditText _startEditText;
        private EditText _endEditText;
        private TripSummary TripSummary { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments == null || !Arguments.ContainsKey(SysPrefs.SERIALIZABLE_TRIPSUMMARY)) return;
            try
            {
                TripSummary =
                    JsonConvert.DeserializeObject<TripSummary>(
                        Arguments.GetString(SysPrefs.SERIALIZABLE_TRIPSUMMARY));
            }
            catch (Exception e)
            {
                LogService.ExceptionLog("Error deserializing tripsummary in tripsummaryfragment");
                LogService.ExceptionLog(e.Message);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_tripsummary, container, false);
            if (TripSummary == null) return view;

            var titleString = DatabaseService.GetInstance().GetRiverName(TripSummary.RiverId);
            titleString += " - ";
            titleString += TripSummary.StartDateTime.ToShortDateString();
            var timesString = TripSummary.StartDateTime.ToStringHrsMinsAmPm();
            timesString += " - " + TripSummary.EndTime.ToStringHrsMinsAmPm();
            timesString += " | " + TripSummary.EndTime.Subtract(TripSummary.StartDateTime).ToStringHrsOrMins();
            view.FindViewById<TextView>(Resource.Id.tripsummary_title).Text =
                titleString;
            view.FindViewById<TextView>(Resource.Id.tripsummary_times).Text =
                timesString;
            return view;
        }

        public static TripSummaryFragment NewInstance()
        {
            var fragment = new TripSummaryFragment();
            return fragment;
        }
    }
}