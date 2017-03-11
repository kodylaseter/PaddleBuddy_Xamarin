using Android.OS;
using Android.Views;

namespace PaddleBuddy.Droid.Fragments
{
    public class RegisterFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_register, container, false);

            return view;
        }

        public static RegisterFragment NewInstance()
        {
            var fragment = new RegisterFragment();
            return fragment;
        }
    }
}