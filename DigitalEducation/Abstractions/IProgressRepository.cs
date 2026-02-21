namespace DigitalEducation
{
    public interface IProgressRepository
    {
        UserProgress Load();
        void Save(UserProgress progress);
        string GetFilePath();
    }
}