using System.Text;
using System.Text.RegularExpressions;

namespace WebSocketServer
{
    public static class HTMLFormatter
    {
        public static string GetWordMapFromHTML(string htmlContent)
        {
            string[] words = GetWordsFromHTMLContent(htmlContent);
            Dictionary<string, int> wordMap = GetWordMapFromWords(words);
            return GetStringFromWordList(wordMap);
        }

        private static string[] GetWordsFromHTMLContent(string content)
        {
            List<string> words = new List<string>();

            // Gets all enclosed HTML elements (<...>)
            Regex regexHTML = new Regex(@"<[^<>]*>", RegexOptions.Compiled);

            // Gets all spaces
            Regex regexSpaces = new Regex(@"\s+", RegexOptions.Compiled);

            // Gets everything that's not in the german alphabet
            Regex regexSpecialChars = new Regex(@"[^a-zA-ZäÄöÖüÜß]", RegexOptions.Compiled);

            string[] splitHTML = regexHTML.Split(content);
            foreach (string nonHTMLString in splitHTML)
            {
                string[] splitSpaces = regexSpaces.Split(nonHTMLString);
                foreach (string nonSpaceString in splitSpaces)
                {
                    // can't change nonSpaceString (... = ..., ref) because it's an iterator variable
                    string word = nonSpaceString;

                    // Delete non-breaking space '&nbsp;'
                    if (word.Contains("&nbsp;"))
                    {
                        word = word.Remove(word.IndexOf("&nbsp;"), 6);
                    }

                    // Delete any remaining special characters
                    if (regexSpecialChars.IsMatch(word))
                    {
                        RemoveSpecialCharacters(ref word);
                    }

                    // Skip if the word is empty
                    if (word == string.Empty)
                    {
                        continue;
                    }

                    words.Add(word);
                }
            }

            return words.ToArray();
        }

        private static void RemoveSpecialCharacters(ref string stringToModify)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char character in stringToModify)
            {
                if ((character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || character is ('ä' or 'ö' or 'ü' or 'ß'))
                {
                    stringBuilder.Append(character);
                }
            }

            stringToModify = stringBuilder.ToString();
        }

        private static Dictionary<string, int> GetWordMapFromWords(string[] words)
        {
            Dictionary<string, int> wordMap = new Dictionary<string, int>();

            foreach (string word in words)
            {
                if (!wordMap.ContainsKey(word))
                {
                    wordMap.Add(word, 1);
                }
                else
                {
                    wordMap[word]++;
                }
            }

            return wordMap;
        }

        private static string GetStringFromWordList(Dictionary<string, int> wordMap)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");

            foreach (KeyValuePair<string, int> word in wordMap)
            {
                // Produces   \"word\": count,   that will be converted into   "word": count,   by the client (JSON Converter)
                stringBuilder.Append(@"\""" + word.Key + @"\"": " + word.Value + @", ");
            }

            // Remove ", " after the last item
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
