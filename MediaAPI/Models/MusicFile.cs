using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MediaAPI.Models
{
    public partial class MusicFile
    {
        public int MusicFileId { get; set; }
        public string Cover { get; set; }
        public string Artist { get; set; }
        public int MediaFileId { get; set; }

        public MediaFile MediaFile { get; set; }

        #region validation

        public static bool ValidateModel(MusicFile _data)
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
                Debug.WriteLine($"Ошибка при валидации MusicFile {_ex}");
                return false;
            }
            return true;
        }

        #endregion
    }
}
