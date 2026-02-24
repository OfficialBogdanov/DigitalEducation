using System;
using System.Windows;

namespace DigitalEducation
{
    public interface ILessonCardFactory
    {
        FrameworkElement CreateLessonCard(
            LessonDataModel lesson,
            int number,
            Action<LessonDataModel> onEdit,
            Action<LessonDataModel> onDelete,
            Action<LessonDataModel> onStart);
    }
}