namespace MediaToolkit
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>   Values that represent fmpeg tasks. </summary>
    internal enum FFmpegTask
    {
        /// <summary>   An enum constant representing the convert option. </summary>
        Convert,

        /// <summary>   An enum constant representing the get meta data option. </summary>
        GetMetaData,

        /// <summary>   An enum constant representing the get thumbnail option. </summary>
        GetThumbnail,
        
        /// <summary>   An enum constant representing the extract frames option. </summary>
        ExtractFrames,
        
        /// <summary>   An enum constant representing the convertion of frames to video option. </summary>
        FramesToVideo
    }
}
