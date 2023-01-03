namespace spiderhead.Models
{
    class CurrentMedia
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public byte[] Art { get; set; } = Tools.Images.BitmapToByteArray(Properties.Resources.cd);
        public bool HasDefaultArtwork { get; set; } = true;

        public CurrentMedia() { }

        public CurrentMedia(string name, string artist, string album)
        {
            Name = name;
            Artist = artist;
            Album = album;
        }

        public CurrentMedia(string name, string artist, string album, byte[] art)
        {
            Name = name;
            Artist = artist;
            Album = album;
            Art = art;
            HasDefaultArtwork = false;
        }
    }
}
