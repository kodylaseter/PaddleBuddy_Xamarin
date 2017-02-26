using Android.OS;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Models;

namespace PaddleBuddy.Droid.Fragments
{
    public class TripSummaryFragment : BaseFragment
    {
        private EditText _startEditText;
        private EditText _endEditText;
        private TripSummary _tripSummary;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_tripsummary, container, false);
            return view;
        }

        public static TripSummaryFragment NewInstance(TripSummary tripSummary = null)
        {
            var fragment = new TripSummaryFragment();
            if (tripSummary != null)
                fragment._tripSummary = tripSummary;
            return fragment;
        }
    }
}