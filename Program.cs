using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace FantasyQuizApp
{
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime BirthDate { get; set; }
        public List<Result> Results { get; set; } = new List<Result>();
    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public List<int> CorrectAnswers { get; set; }
    }

    public class Quiz
    {
        public string Name { get; set; }
        public List<Question> Questions { get; set; }
    }

    public class Result
    {
        public string QuizName { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }
    }

    class Program
    {
        static List<User> users = new List<User>();
        static List<Quiz> quizzes = new List<Quiz>();
        static string usersFile = "users.json"; 

        static void Main()
        {
            LoadUsers();
            InitializeQuizzes();

            User currentUser = null;

            while (true)
            {
                Console.WriteLine("1. Login\n2. Register\n3. Exit");
                string choice = Console.ReadLine();
                if (choice == "1") currentUser = Login();
                else if (choice == "2") currentUser = Register();
                else if (choice == "3") break;
                else Console.WriteLine("Invalid option. Try again.");

                if (currentUser != null)
                {
                    MainMenu(currentUser);
                    currentUser = null; 
                }
            }

            SaveUsers();
            Console.WriteLine("Goodbye!");
        }

        static void LoadUsers()
        {
            if (File.Exists(usersFile))
            {
                string json = File.ReadAllText(usersFile);
                users = JsonSerializer.Deserialize<List<User>>(json);
            }
        }

        static void SaveUsers()
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(usersFile, json);
        }

        static User Register()
        {
            Console.Write("Enter login: ");
            string login = Console.ReadLine();

            if (users.Exists(u => u.Login == login))
            {
                Console.WriteLine("Login already exists!");
                return null;
            }

            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            Console.Write("Enter birth date (yyyy-mm-dd): ");
            DateTime birthDate = DateTime.Parse(Console.ReadLine());

            var user = new User { Login = login, Password = password, BirthDate = birthDate };
            users.Add(user);
            SaveUsers();
            Console.WriteLine("Registration successful!");
            return user;
        }

        static User Login()
        {
            Console.Write("Login: ");
            string login = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            var user = users.Find(u => u.Login == login && u.Password == password);
            if (user == null)
                Console.WriteLine("Wrong login or password!");
            return user;
        }

        static void MainMenu(User user)
        {
            while (true)
            {
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1. Start new quiz");
                Console.WriteLine("2. View my results");
                Console.WriteLine("3. Top-20");
                Console.WriteLine("4. Settings");
                Console.WriteLine("5. Logout");

                string choice = Console.ReadLine();
                if (choice == "1") StartQuiz(user);
                else if (choice == "2") ShowResults(user);
                else if (choice == "3") ShowTop20();
                else if (choice == "4") ChangeSettings(user);
                else if (choice == "5") break;
                else Console.WriteLine("Invalid choice!");
            }
        }

        static void InitializeQuizzes()
        {
            quizzes = new List<Quiz>
            {
                new Quiz
                {
                    Name = "Harry Potter",
                    Questions = new List<Question>
                    {
                        new Question { Text = "Who is the Half-Blood Prince?", Options = new List<string>{"Harry Potter","Severus Snape","Tom Riddle","Draco Malfoy"}, CorrectAnswers = new List<int>{2} },
                        new Question { Text = "What is the core of Harry's wand?", Options = new List<string>{"Phoenix feather","Dragon heartstring","Unicorn hair","Thestral tail hair"}, CorrectAnswers = new List<int>{1} },
                        new Question { Text = "Who did Harry fight in the Triwizard Tournament?", Options = new List<string>{"Cedric Diggory","Viktor Krum","Fleur Delacour","All of the above"}, CorrectAnswers = new List<int>{4} },
                      
                    }
                },
                new Quiz
                {
                    Name = "Lord of the Rings",
                    Questions = new List<Question>
                    {
                        new Question { Text = "Who forged the One Ring?", Options = new List<string>{"Sauron","Elrond","Gandalf","Saruman"}, CorrectAnswers = new List<int>{1} },
                        new Question { Text = "Which race is Legolas?", Options = new List<string>{"Dwarf","Elf","Human","Hobbit"}, CorrectAnswers = new List<int>{2} },
                        new Question { Text = "Who carries the ring to Mount Doom?", Options = new List<string>{"Frodo","Sam","Gollum","Bilbo"}, CorrectAnswers = new List<int>{1} },
                      
                    }
                }
            };
        }

        static void StartQuiz(User user)
        {
            Console.WriteLine("\nChoose a quiz:");
            for (int i = 0; i < quizzes.Count; i++)
                Console.WriteLine($"{i + 1}. {quizzes[i].Name}");
            Console.WriteLine($"{quizzes.Count + 1}. Random mixed quiz");

            int choice = int.Parse(Console.ReadLine());
            List<Question> questionsToAsk = new List<Question>();
            Random rnd = new Random();

            if (choice <= quizzes.Count)
                questionsToAsk = quizzes[choice - 1].Questions;
            else
            {
                foreach (var q in quizzes)
                    questionsToAsk.Add(q.Questions[rnd.Next(q.Questions.Count)]);
            }

            int score = 0;
            foreach (var q in questionsToAsk)
            {
                Console.WriteLine($"\n{q.Text}");
                for (int i = 0; i < q.Options.Count; i++)
                    Console.WriteLine($"{i + 1}. {q.Options[i]}");

                Console.WriteLine("Enter correct answer numbers separated by space:");
                var answers = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);

                if (answers.Length == q.CorrectAnswers.Count && !answers.Except(q.CorrectAnswers).Any())
                    score++;
            }

            Console.WriteLine($"\nYou scored {score}/{questionsToAsk.Count} correct answers!");

            user.Results.Add(new Result
            {
                QuizName = choice <= quizzes.Count ? quizzes[choice - 1].Name : "Mixed",
                Score = score,
                Date = DateTime.Now
            });

            SaveUsers();
        }

        static void ChangeSettings(User user)
        {
            Console.WriteLine("1. Change password\n2. Change birth date");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Enter new password: ");
                user.Password = Console.ReadLine();
                Console.WriteLine("Password changed!");
            }
            else if (choice == "2")
            {
                Console.Write("Enter new birth date (yyyy-mm-dd): ");
                user.BirthDate = DateTime.Parse(Console.ReadLine());
                Console.WriteLine("Birth date changed!");
            }

            SaveUsers();
        }

        static void ShowResults(User user)
        {
            Console.WriteLine("\nYour past results:");
            foreach (var r in user.Results)
                Console.WriteLine($"{r.QuizName} - {r.Score} points - {r.Date}");
        }

        static void ShowTop20()
        {
            var top = users.SelectMany(u => u.Results).OrderByDescending(r => r.Score).Take(20);
            Console.WriteLine("\nTop-20 players:");
            foreach (var r in top)
                Console.WriteLine($"{r.QuizName} - {r.Score} points - {r.Date}");
        }
    }
}

