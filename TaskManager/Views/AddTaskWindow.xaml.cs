using System.Windows;
using TaskManager.Models;

namespace TaskManager.Views
{
    public partial class AddTaskWindow : Window
    {
        public KanbanTask Task { get; private set; }
        public bool IsDeleteRequested { get; private set; }

        public AddTaskWindow(KanbanTask task = null)
        {
            InitializeComponent();
            
            if (task != null)
            {
                Task = task;
                TitleTextBox.Text = task.Title;
                DescriptionTextBox.Text = task.Description;
                DeleteButton.Visibility = Visibility.Visible;
                Title = "Редактировать задачу";
            }
            else
            {
                DeleteButton.Visibility = Visibility.Collapsed;
                Title = "Добавить задачу";
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Task == null)
            {
                Task = new KanbanTask
                {
                    Title = TitleTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Status = KanbanTaskStatus.Backlog
                };
            }
            else
            {
                Task.Title = TitleTextBox.Text;
                Task.Description = DescriptionTextBox.Text;
            }

            DialogResult = true;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            IsDeleteRequested = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 