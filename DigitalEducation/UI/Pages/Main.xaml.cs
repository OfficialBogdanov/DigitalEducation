using DigitalEducation.Core.Managers;
using System;
using System.Windows;

namespace DigitalEducation.UI.Pages
{
    public partial class Main : Window
    {
        private Base _baseLogic;

        public Main()
        {
            InitializeComponent();
            _baseLogic = new Base(this, "home");
        }

        protected override void OnClosed(EventArgs e)
        {
            _baseLogic.Cleanup();
            base.OnClosed(e);
        }

        private void StartLearning_Click(object sender, RoutedEventArgs e)
        {
            _baseLogic.ShowModal(
                title: "Начинаем обучение",
                message: "Вы переходите к списку курсов. Продолжить?",
                confirmText: "Продолжить",
                cancelText: "Отмена",
                onConfirm: () => _baseLogic.NavigateTo("courses")
            );
        }

        private void AllCourses_Click(object sender, RoutedEventArgs e)
        {
            _baseLogic.ShowModal(
                title: "Все курсы",
                message: "Открыть список всех доступных курсов?",
                confirmText: "Открыть",
                cancelText: "Закрыть",
                onConfirm: () => _baseLogic.NavigateTo("courses")
            );
        }
    }
}