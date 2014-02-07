namespace MediaToolkit.Model
{
    public class MediaFile
    {
        public MediaFile(){}

        public MediaFile(string filename)
        {
            Filename = filename;
        }
        
        public string Filename { get; set; }

        public Metadata Metadata { get; internal set; }
        
    }

}
