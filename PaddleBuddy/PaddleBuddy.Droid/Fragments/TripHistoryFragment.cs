using Android.OS;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Droid.Adapters;
using PaddleBuddy.Droid.Controls;

namespace PaddleBuddy.Droid.Fragments
{
    public class TripHistoryFragment : BaseFragment
    {
        private ListView _listView;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_triphistory, container, false);
            var adapter = new TripSummaryAdapter(Activity);
            _listView = view.FindViewById<ListView>(Resource.Id.triphistory_listview);
            _listView.Divider = null;
            _listView.Adapter = adapter;
            _listView.ItemClick += OnTripSummaryClicked;
            _listView.ItemSelected += OnTripSummarySelected;
            return view;
        }

        private void OnTripSummaryClicked(object sender, AdapterView.ItemClickEventArgs e)
        {
            ((TripSummaryCardView)e.View).ToggleExpansion();
            _listView.Invalidate();
        }

        private void OnTripSummarySelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ((TripSummaryCardView)e.View).ToggleExpansion();
        }

        public static TripHistoryFragment NewInstance()
        {
            var fragment = new TripHistoryFragment();
            return fragment;
        }
    }
}