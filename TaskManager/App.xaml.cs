using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Windows;
using System.Windows.Threading;
using TaskManager.Data;
using TaskManager.Services;
using TaskManager.Views;
using Microsoft.EntityFrameworkCore;

namespace TaskManager
{
    public partial class App : PrismApplication
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Необработанное исключение: {e.Exception.Message}\n\nПодробности: {e.Exception.StackTrace}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}\n\nПодробности: {ex.StackTrace}",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override Window CreateShell()
        {
            try
            {
                var window = Container.Resolve<MainWindow>();
                if (window == null)
                {
                    throw new Exception("Не удалось создать главное окно");
                }
                return window;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании главного окна: {ex.Message}\n\nПодробности: {ex.StackTrace}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return null;
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            try
            {
                // Создаем опции для DbContext
                var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                    .UseSqlite("Data Source=TaskManager.db")
                    .EnableSensitiveDataLogging()
                    .Options;

                // Регистрируем DbContext
                containerRegistry.RegisterInstance(options);
                containerRegistry.Register<TaskManagerDbContext>();

                // Регистрируем сервисы
                containerRegistry.Register<ITaskService, TaskService>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации сервисов: {ex.Message}\n\nПодробности: {ex.StackTrace}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                
                var context = Container.Resolve<TaskManagerDbContext>();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации приложения: {ex.Message}\n\nПодробности: {ex.StackTrace}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
} 