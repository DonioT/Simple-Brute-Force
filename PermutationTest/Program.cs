using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PermutationTest
{
    class Program
    {
        private static string passwordMemory = "password";
        private static string rootDirectory = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        private static HttpClient _httpClient = new HttpClient();
        private static void Main(string[] args)
        {
            Dictionary<char, IEnumerable<char>> substitutions = new Dictionary<char, IEnumerable<char>>();
            //get dinstict letters into a char array to fetch all possible chars for char
            char[] passwordParts = passwordMemory.Distinct().ToArray();

            for (int i = 0; i < passwordParts.Count(); i++)
            {
                substitutions.Add(passwordParts[i], PossibleChars(passwordParts[i]).ToArray());
            }

            //get word permutations
            List<string> listOfCombinations = new List<string>(GetCombinations(passwordMemory, substitutions));

            bool fileExists = TextFileExists("passwords.txt") ? true : CreateTextFile("passwords.txt", listOfCombinations);

            if (fileExists)
            {
                var passwordList = ReadFileLines("passwords.txt");
                var url = InitializeRequests(passwordList).Result.Content.ReadAsStringAsync();
                if (url != null)
                {
                   HttpResponseMessage response = SendZip(url.Result);
                }
            }

            Console.Read();
        }

        public static HttpResponseMessage SendZip(string url)
        {
            try
            {
                byte[] AsBytes = File.ReadAllBytes(rootDirectory + "/Donio_TeixeiraREAL.zip");
                string AsBase64String = Convert.ToBase64String(AsBytes);

                string json = "{'Data':'" + AsBase64String + "'}";

                HttpResponseMessage response = new HttpResponseMessage();
                response = _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json")).Result;

                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }


        public static List<string> ReadFileLines(string name)
        {
            return File.ReadAllLines(rootDirectory + "/" + name).ToList();
        }

        public static bool TextFileExists(string name)
        {
            return File.Exists(rootDirectory + "/" + name);
        }

        public static bool CreateTextFile(string name, List<string> content)
        {
            try
            {
                File.WriteAllLines(rootDirectory + "/" + name, content);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        //set substitution chars
        public static List<char> PossibleChars(char c)
        {
            List<char> list = new List<char> {
                char.ToUpper(c),
                char.ToLower(c)
        };
            switch (c)
            {
                case ('a'):
                    list.AddRange(new List<char> { '@' });
                    break;
                case ('o'):
                    list.AddRange(new List<char> { '0' });
                    break;
                case ('s'):
                    list.AddRange(new List<char> { '5' });
                    break;
            }

            return list;
        }


        //create task list
        public static Task<HttpResponseMessage> InitializeRequests(List<string> list)
        {
            var tasks = new List<Task<HttpResponseMessage>>();
            try
            {
                    for (int i = 0; i < list.Count; i++)
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "admin", list[i]))));
                        tasks.Add(_httpClient.GetAsync("removedurl"));
                    }
               
                return EvaluateTasks(tasks);
            }
            catch (Exception e)
            {
                return null;
            }


        }

        //execute task list and evaluate task results as they complete, return task that got correct status code
        private static async Task<HttpResponseMessage> EvaluateTasks(List<Task<HttpResponseMessage>> tasks)
        {
            while (tasks.Count > 0)
            {
                Task<HttpResponseMessage> task = await Task.WhenAny(tasks);
                if (task.Status == TaskStatus.RanToCompletion && task.Result.StatusCode == HttpStatusCode.OK)
                {
                    return task.Result;
                }

                tasks.Remove(task);
            }
            return null;
        }


        //get word permutations
        public static IEnumerable<string> GetCombinations(string word, Dictionary<char, IEnumerable<char>> substitutions)
        {
            IEnumerable<string> GetCombinations(string original, string current, int index)
            {
                if (index == original.Length)
                {
                    yield return current;
                }
                else
                {
                    if (substitutions.ContainsKey(original[index]))
                    {
                        var chars = substitutions[original[index]];
                        foreach (var c in chars)
                        {
                            foreach (var combination in
                                GetCombinations(original, current + c, index + 1))
                            {
                                yield return combination;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(word)) return Enumerable.Repeat(word, 1);

            var lowerCased = word.ToLower();
            return GetCombinations(lowerCased, string.Empty, 0);
        }
    }
}
