namespace DigitalEducation
{
    public interface IProgressRepository
    {
        UserProgressModel Load();
        void Save(UserProgressModel progress);
        string GetFilePath();
    }
}