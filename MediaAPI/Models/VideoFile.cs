using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MediaAPI.Models
{
    public partial class VideoFile
    {
        public int VideoFileId { get; set; }
        public string Preview { get; set; }
        public int MediaFileId { get; set; }

        public virtual MediaFile MediaFile { get; set; }

        #region validation

        public static bool ValidateModel(VideoFile _data)
        {
            try
            {
                if (_data == null ||
                    !MediaFile.ValidateModel(_data.MediaFile)
                    )
                {
                    return false;
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine($"Ошибка при валидации VideoFile {_ex}");
                return false;
            }
            return true;
        }

        #endregion
    }
}
