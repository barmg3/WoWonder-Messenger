using DT.Xamarin.Agora;

namespace WoWonder.Frameworks.Agora
{
    public class AgoraRtcHandler : IRtcEngineEventHandler
    {
        private readonly AgoraVideoCallActivity Context;

        public AgoraRtcHandler(AgoraVideoCallActivity activity)
        {
            Context = activity;
        }

        public override void OnFirstRemoteVideoDecoded(int p0, int p1, int p2, int p3)
        {
            base.OnFirstRemoteVideoDecoded(p0, p1, p2, p3);
            Context.OnFirstRemoteVideoDecoded(p0, p1, p2, p3);
        }

        public override void OnConnectionLost()
        {
            base.OnConnectionLost();
            Context.OnConnectionLost();
        }

        public override void OnUserOffline(int p0, int p1)
        {
            base.OnUserOffline(p0, p1);
            Context.OnUserOffline(p0, p1);
        }

        public override void OnUserMuteVideo(int p0, bool p1)
        {
            base.OnUserMuteVideo(p0, p1);
            Context.OnUserMuteVideo(p0, p1);
        }

        public override void OnFirstLocalVideoFrame(int p0, int p1, int p2)
        {
            base.OnFirstLocalVideoFrame(p0, p1, p2);
            Context.OnFirstLocalVideoFrame(p0, p1, p2);
        }

        public override void OnNetworkQuality(int p0, int p1, int p2)
        {
            base.OnNetworkQuality(p0, p1, p2);
            Context.OnNetworkQuality(p0, p1, p2);
        }

        public override void OnUserJoined(int p0, int p1)
        {
            base.OnUserJoined(p0, p1);
            Context.OnUserJoined(p0, p1);
        }

        public override void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {
            base.OnJoinChannelSuccess(channel, uid, elapsed);
            Context.OnJoinChannelSuccess(channel, uid, elapsed);
        }
    }
}