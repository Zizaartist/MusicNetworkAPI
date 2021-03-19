using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Models.EnumModels
{
    public enum MediaExtension
    {
        //video
        v3gp = 0,
        vmp4 = 1,
        vmkv = 2,
        vwebm = 3,
        //music
        mmp3 = 10,
        mwav = 11,
        mogg = 12,
        //image
        ijpg = 20,
        ijpeg = 21,
        ipng = 22,
        igif = 23,
        iwebp = 24
    }

    public class MediaExtensionDictionaries 
    {
        public static Dictionary<MediaExtension, string> MediaExtensionToString = new Dictionary<MediaExtension, string>()
        {
            { MediaExtension.v3gp, ".3gp" },
            { MediaExtension.vmp4, ".mp4" },
            { MediaExtension.vmkv, ".mkv" },
            { MediaExtension.vwebm, ".webm" },

            { MediaExtension.mmp3, ".mp3" },
            { MediaExtension.mwav, ".wav" },
            { MediaExtension.mogg, ".ogg" },

            { MediaExtension.ijpg, ".jpg" },
            { MediaExtension.ipng, ".png" },
            { MediaExtension.igif, ".gif" },
            { MediaExtension.iwebp, ".webp" }
        };

        public static Dictionary<string, MediaExtension> StringToMediaExtension = new Dictionary<string, MediaExtension>()
        {
            { ".3gp", MediaExtension.v3gp },
            { ".mp4", MediaExtension.vmp4 },
            { ".mkv", MediaExtension.vmkv },
            { ".webm", MediaExtension.vwebm },

            { ".mp3", MediaExtension.mmp3 },
            { ".wav", MediaExtension.mwav },
            { ".ogg", MediaExtension.mogg },

            { ".jpg", MediaExtension.ijpg },
            { ".jpeg", MediaExtension.ijpeg },
            { ".png", MediaExtension.ipng },
            { ".gif", MediaExtension.igif }
        };

        public static Dictionary<string, List<byte[]>> ExtensionToSignature = new Dictionary<string, List<byte[]>>()
        {
            //{ ".3gp", new List<byte[]> { new byte[] { 0x66, 0x74, 0x79, 0x70, 0x33, 0x67, 0x70 } } },

            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
        };
    }
}
