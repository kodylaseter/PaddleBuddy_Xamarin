using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace PaddleBuddy.Droid.Fragments
{
    public class PlanFragment : BaseFragment
    {
        private EditText _startEditText;
        private EditText _endEditText;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_plan, container, false);
            _startEditText = (EditText) view.FindViewById(Resource.Id.start_edittext);
            _endEditText = (EditText) view.FindViewById(Resource.Id.end_edittext);
            return view;
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