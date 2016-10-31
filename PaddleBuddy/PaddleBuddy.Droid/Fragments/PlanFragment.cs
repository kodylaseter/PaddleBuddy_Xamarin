using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;

namespace PaddleBuddy.Droid.Fragments
{
    public class PlanFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_plan, container, false);
        }

        public static PlanFragment NewInstance(int startId = int.MaxValue)
        {
            var fragment = new PlanFragment();
            if (startId == int.MaxValue) return fragment;
            var args = new Bundle();
            args.PutInt("startId", startId);
            fragment.Arguments = args;
            return fragment;
        }
    }
}