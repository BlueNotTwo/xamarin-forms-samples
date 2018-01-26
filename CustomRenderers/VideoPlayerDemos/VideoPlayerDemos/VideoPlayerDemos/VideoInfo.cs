using MediaHelpers;

namespace VideoPlayerDemos
{
    public class VideoInfo
    {
        public string DisplayName { set; get; }


        // TODO: Remove this!
        public string Uri { set; get; }


        public VideoSource VideoSource { set; get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
