using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;

namespace PaddleBuddy.Droid.Fragments
{
    public class TripSummaryFragment : BaseFragment
    {
        private EditText _startEditText;
        private EditText _endEditText;
        private TripSummary _tripSummary;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments == null || !Arguments.ContainsKey(SysPrefs.SERIALIZABLE_TRIPSUMMARY)) return;
            try
            {
                _tripSummary =
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
            if (_tripSummary != null)
            {
                view.FindViewById<TextView>(Resource.Id.river_name).Text =
                    DatabaseService.GetInstance().GetRiverName(_tripSummary.RiverId);
                view.FindViewById<TextView>(Resource.Id.trip_date).Text = _tripSummary.StartDateTime.ToString("YY-MM-DD/dd/yyyy");

            }
            return view;
        }

        public static TripSummaryFragment NewInstance()
        {
            var fragment = new TripSummaryFragment();
            return fragment;
        }
    }
}