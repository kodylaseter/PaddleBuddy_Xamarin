using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;
using PaddleBuddy.Droid.Controls;

namespace PaddleBuddy.Droid.Fragments
{
    public class TripSummaryFragment : BaseFragment
    {
        private TripSummary TripSummary { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments == null || !Arguments.ContainsKey(PBPrefs.SERIALIZABLE_TRIPSUMMARY)) return;
            try
            {
                TripSummary =
                    JsonConvert.DeserializeObject<TripSummary>(
                        Arguments.GetString(PBPrefs.SERIALIZABLE_TRIPSUMMARY));
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
            var tripSummaryCardView = new TripSummaryCardView(Context, false);
            tripSummaryCardView.UpdateData(TripSummary);
            view.FindViewById<LinearLayout>(Resource.Id.summary_container).AddView(tripSummaryCardView);

            return view;
        }

        public static TripSummaryFragment NewInstance()
        {
            var fragment = new TripSummaryFragment();
            return fragment;
        }
    }
}