using Android.OS;
using Android.Support.V4.App;
using Android.Views;

namespace PaddleBuddy.Droid.Fragments
{
    public class MapFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_map, container, false);
        }

        public static MapFragment NewInstance()
        {
            return new MapFragment();
        }
    }
}