using Android.OS;
using Android.Support.V4.App;
using Android.Views;

namespace PaddleBuddy.Droid.Fragments
{
    public class PlanFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_map, container, false);
        }

        public static PlanFragment NewInstance()
        {
            return new PlanFragment();
        }
    }
}