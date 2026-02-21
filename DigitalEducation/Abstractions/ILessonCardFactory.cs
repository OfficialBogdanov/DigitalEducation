using System;
using System.Windows;

namespace DigitalEducation
{
    public interface ILessonCardFactory
    {
        FrameworkElement CreateLessonCard(
            LessonData lesson,
            int number,
            Action<LessonData> onEdit,
            Action<LessonData> onDelete,
            Action<LessonData> onStart);
    }
}