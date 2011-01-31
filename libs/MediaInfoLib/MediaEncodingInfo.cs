#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaInfoLib
{
    public enum MediaType { Video, Audio, Image };
    public class MediaEncodingInfo
    {
        private MediaType type;

        public MediaEncodingInfo(MediaType t)
        {
            type = t;
        }
        //Audio+Video
        public long Length { set; get; }
        public string Format { set; get; }
        //Video
        public string VideoCodec { set; get; }
        public string FrameRate { set; get; }
        public long VideoBitrate { set; get; }
        //Video+image
        public int Height { set; get; }
        public int Width { set; get; }
        //Audio
        public string AudioCodec { set; get; }
        public long AudioBitrate { set; get; }

        public static MediaEncodingInfo GetMediaInfo(string path)
        {
            try
            {
                MediaInfo mi = new MediaInfo();
                if (mi.Open(path) != 0)
                {
                    int videoStreams = mi.Count_Get(StreamKind.Video);
                    int audioStreams = mi.Count_Get(StreamKind.Audio);
                    int imageStreams = mi.Count_Get(StreamKind.Image);

                    MediaEncodingInfo info = null;

                    if (imageStreams > 0)
                    {
                        info = new MediaEncodingInfo(MediaType.Image);
                        info.Height = (int)ParseLong(mi.Get(StreamKind.Image, 0, "Height"));
                        info.Width = (int)ParseLong(mi.Get(StreamKind.Image, 0, "Width"));
                        info.Format = mi.Get(StreamKind.General, 0, "Format");
                    }
                    else
                    {
                        if (videoStreams > 0)
                        {
                            info = new MediaEncodingInfo(MediaType.Video);

                            info.Format = mi.Get(StreamKind.General, 0, "Format");
                            info.Length = ParseLong(mi.Get(StreamKind.General, 0, "Duration"));

                            info.VideoCodec = mi.Get(StreamKind.Video, 0, "Format");
                            info.FrameRate = mi.Get(StreamKind.Video, 0, "FrameRate");
                            info.VideoBitrate = ParseLong(mi.Get(StreamKind.Video, 0, "BitRate"));
                            info.Width = (int)ParseLong(mi.Get(StreamKind.Video, 0, "Width"));
                            info.Height = (int)ParseLong(mi.Get(StreamKind.Video, 0, "Height"));
                        }

                        if (audioStreams > 0)
                        {
                            if (null == info)
                            {
                                info = new MediaEncodingInfo(MediaType.Audio);
                                info.Format = mi.Get(StreamKind.General, 0, "Format");
                                info.Length = ParseLong(mi.Get(StreamKind.General, 0, "Duration"));
                            }
                            info.AudioCodec = mi.Get(StreamKind.Audio, 0, "Format");
                            info.AudioBitrate = ParseLong(mi.Get(StreamKind.Audio, 0, "BitRate"));
                        }
                    }
                    mi.Close();
                    return info;
                }
            }
            catch { }
            return null;
        }

        private static long ParseLong(string text)
        {
            long l = 0;
            long.TryParse(text, out l);
            return l;
        }
    }
}
