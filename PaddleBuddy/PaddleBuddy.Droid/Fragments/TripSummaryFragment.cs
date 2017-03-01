using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Controls;

namespace PaddleBuddy.Droid.Fragments
{
    public class TripSummaryFragment : BaseFragment
    {
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
            var view = new LinearLayout(Context);
            if (TripSummary == null) return view;
            view.SetPadding(5,5,5,5);
            view.AddView(new TripSummaryCardView(Context, TripSummary, false));
            return view;
        }

        public static TripSummaryFragment NewInstance()
        {
            var fragment = new TripSummaryFragment();
            return fragment;
        }
    }
}