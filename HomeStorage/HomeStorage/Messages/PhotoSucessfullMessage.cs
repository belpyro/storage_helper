namespace HomeStorage.Messages
{
    public class PhotoSucessfullMessage
    {
        public PhotoSucessfullMessage(string name)
        {
            ImageName = name;
        }

        public string ImageName { get; set; }
    }
}
