using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using PaddleBuddy.Droid.Adapters;

namespace PaddleBuddy.Droid.Fragments
{
    public class TripHistoryFragment : BaseFragment
    {
        private RecyclerView RecyclerView { get; set; }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_triphistory, container, false);
            var adapter = new TripSummaryAdapter(Activity); 
            RecyclerView = view.FindViewById<RecyclerView>(Resource.Id.triphistory_recyclerview);
            RecyclerView.SetAdapter(adapter);
            RecyclerView.SetLayoutManager(new LinearLayoutManager(Context));
            return view;
        }

        public static TripHistoryFragment NewInstance()
        {
            var fragment = new TripHistoryFragment();
            return fragment;
        }
    }
}