using GalaSoft.MvvmLight.Messaging;
using PaddleBuddy.Core.Models.Messages;

namespace PaddleBuddy.Core.Services
{
    public class MessengerService
    {
        public static IMessenger Messenger => GalaSoft.MvvmLight.Messaging.Messenger.Default;

        public static void SendDbReady()
        {
            Messenger.Send(new DbReadyMessage());
        }
    }
}
