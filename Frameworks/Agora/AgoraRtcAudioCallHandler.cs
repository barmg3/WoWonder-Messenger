using DT.Xamarin.Agora;

namespace WoWonder.Frameworks.Agora
{
    public class AgoraRtcAudioCallHandler : IRtcEngineEventHandler
    {
        private readonly AgoraAudioCallActivity Context;

        public AgoraRtcAudioCallHandler(AgoraAudioCallActivity activity)
        {
            Context = activity;
        }

        public override void OnConnectionLost()
        {
            base.OnConnectionLost();
            Context.OnConnectionLost();
        }

        public override void OnUserOffline(int uid, int reason)
        {
            base.OnUserOffline(uid, reason);
            Context.OnUserOffline(uid, reason);
        }

        public override void OnNetworkQuality(int uid, int txQuality, int rxQuality)
        {
            base.OnNetworkQuality(uid, txQuality, rxQuality);
            Context.OnNetworkQuality(uid, txQuality, rxQuality);
        }

        public override void OnUserJoined(int uid, int elapsed)
        {
            base.OnUserJoined(uid, elapsed);
            Context.OnUserJoined(uid, elapsed);
        }

        public override void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {
            base.OnJoinChannelSuccess(channel, uid, elapsed);
            Context.OnJoinChannelSuccess(channel, uid, elapsed);
        }

        public override void OnUserMuteAudio(int uid, bool muted)
        {
            base.OnUserMuteAudio(uid, muted);
            Context.OnUserMuteAudio(uid, muted);
        }
    }
}