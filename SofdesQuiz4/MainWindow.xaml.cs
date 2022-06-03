using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.WinUI.UI.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SofdesQuiz4
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private List<Student> _students = new();
        public List<Student> Students
        {
            get => _students;
            set { _students = value; OnPropertyChanged(); }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            LoadData();
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            var id = (StudentGrid.SelectedItem as Student)?.Id;
            if (id == null)
            {
                await new ContentDialog
                {
                    Title = "Nothing to delete",
                    Content = "You should select a student to delete from the list below first.",
                    CloseButtonText = "Okay",
                    XamlRoot = Content.XamlRoot,
                }.ShowAsync();
                return;
            }
            var response = await new ContentDialog
            {
                Title = "Delete student",
                Content = "Are you sure you want to delete this student?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();

            if (response != ContentDialogResult.Primary) return;
            StudentDb.Delete((int)id);
            Clear();
            LoadData();
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private async void Insert(object sender, RoutedEventArgs e)
        {
            var student = await ParseStudentAsync();
            if (student == null) return;
            StudentDb.Insert(student);
            LoadData();
            Clear();
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            var id = (StudentGrid.SelectedItem as Student)?.Id;
            if (id == null)
            {
                await new ContentDialog
                {
                    Title = "Nothing to update",
                    Content = "You should select a student to update from the list below first.",
                    CloseButtonText = "Okay",
                    XamlRoot = Content.XamlRoot,
                }.ShowAsync();
                return;
            }
            var student = await ParseStudentAsync(id);
            if (student == null) return;
            StudentDb.Update(student);
            Clear();
            LoadData();
        }

        private void Search(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void DisplayStudent(object sender, RoutedEventArgs e)
        {
            if (StudentGrid.SelectedItem is not Student student) return;
            NameInput.Text = student.Name;
            var birthday = student.Birthday;
            BirthdayInput.SelectedDate = new DateTimeOffset(birthday.Year, birthday.Month, birthday.Day, 0, 0, 0,
                TimeSpan.FromHours(8));
            CourseInput.Text = student.Course;
        }

        private async Task<Student> ParseStudentAsync(int? id = null)
        {
            var name = NameInput.Text;
            var birthday = BirthdayInput.SelectedDate;
            var course = CourseInput.Text;

            if (string.IsNullOrEmpty(name) ||
                birthday == null ||
                string.IsNullOrEmpty(course))
            {
                await new ContentDialog
                {
                    Title = "Empty fields",
                    Content = "None of the fields can be empty.",
                    CloseButtonText = "Okay",
                    XamlRoot = Content.XamlRoot,
                }.ShowAsync();
                return null;
            }
            return new Student(name, DateOnly.FromDateTime(((DateTimeOffset)birthday).DateTime), course, id);
        }

        private void Clear()
        {
            NameInput.Text = CourseInput.Text = "";
            BirthdayInput.SelectedDate = null;
            StudentGrid.SelectedItem = null;
        }

        private void LoadData()
        {
            var searchQuery = SearchInput.Text;
            Students = StudentDb.GetAll(searchQuery);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DigitsOnly(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void UpdateAge(object sender, DatePickerSelectedValueChangedEventArgs e)
        {
            var nullableBirthday = e.NewDate;
            if (nullableBirthday != null)
            {
                var birthday = (DateTimeOffset) nullableBirthday;
                var now = DateTimeOffset.Now;
                var age = now.Year - birthday.Year;
                if (now.Month < birthday.Month || (now.Month == birthday.Month && now.Day < birthday.Day))
                {
                    age--;
                }
                AgeInput.Text = age.ToString();
            }
            else
            {
                AgeInput.Text = string.Empty;
            }
        }

        private void HideId(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Id")
            {
                e.Cancel = true;
            }
        }
    }
}
