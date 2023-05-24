using System.Text;

var executionDirectory = Environment.CurrentDirectory;
var text = File.ReadAllText(Path.Combine(Directory.GetParent(executionDirectory).Parent.Parent.FullName, "text.txt"));
var words = text.Split(new char[] {',', '.', ':', ';', '?', '!', ' ', '-', '\n', '\r'})
    .Where(x => !String.IsNullOrWhiteSpace(x))
    .ToArray();
var nextWords = new string?[words.Length];
Array.Copy(words, 1, nextWords, 0, words.Length - 1);
var nextWordsWithIndex = nextWords.Select((x, i) => new
    {
        NextWord = x,
        NextWordIndex = i,
    })
    .ToList();
var everyWordCount = words
    .GroupBy(w => w)
    .ToDictionary(i => i.Key, i => i.Count()); 
var wordsCount = words.Length;

var choices = words.Select((x, i) => new
    {
        Word = x,
        Index = i
    })
    .Join(nextWordsWithIndex, w => w.Index, nw => nw.NextWordIndex, (n, nw) => new
    {
        Word = n.Word,
        NextWord = nw.NextWord
    })
    .GroupBy(x => new { x.Word, x.NextWord })
    .Select(i => new WordChoice(i.Key.Word, i.Key.NextWord, (float)i.Count() / everyWordCount[i.Key.Word], i.Count()))
    .OrderBy(i => i.Probability)
    .ToList();

Console.WriteLine($"Number of words: {words.Length.ToString()}");
Console.WriteLine("Read");
Console.WriteLine(GenerateRandomSentence(choices));
Console.Read();

string GenerateRandomSentence(ICollection<WordChoice> choices, int numberOfWords = 100)
{
    StringBuilder resultStringBuilder = new();
    var capitalLetteredItems = choices.Where(c => c.LeadingWord.ToLower()[0] != c.LeadingWord[0]).ToList();
    int randomBeginningIndex = Random.Shared.Next(0, capitalLetteredItems.Count);
    var word = capitalLetteredItems.ElementAt(randomBeginningIndex);
    AppendPair(word, resultStringBuilder);
    for (int i = 0; i < numberOfWords; ++i)
    {
        var nextChoice = Random.Shared.NextDouble();
        var possibleWords = choices.Where(c => c.LeadingWord == word.NextWord)
            .ToList();
        var probabilities = CumulateProbabilities(possibleWords);
        word = probabilities.Where(i => i.Key > nextChoice).FirstOrDefault().Value
            ?? probabilities.FirstOrDefault().Value;
        AppendPair(word, resultStringBuilder);
    }
    return resultStringBuilder.ToString();
}

List<KeyValuePair<float, WordChoice>> CumulateProbabilities(ICollection<WordChoice> choices)
{
    var probability = 0.0f;
    List<KeyValuePair<float, WordChoice>> probabilities = new();
    foreach (var item in choices)
    {
        probability += item.Probability;
        probabilities.Add(new KeyValuePair<float, WordChoice>(probability, item));
    }
    return probabilities;
}

void AppendPair(WordChoice choice, StringBuilder builder)
{
    if (choice.LeadingWord.ToLower()[0] != choice.LeadingWord[0])
    {
    }
    builder.Append(choice.LeadingWord);
    builder.Append(" ");
}

void PrintNChoices(ICollection<WordChoice> choices, int n = 10)
{
   for (int i = 0; i < n; ++i)
    {
        var item = choices.ElementAt(i);
        Console.WriteLine($"{item.LeadingWord} - {item.NextWord} - {item.Probability * 100}%");
    }    
}

class WordChoice
{
    public string LeadingWord { get; set; }
    public string NextWord { get; set; }
    public float Probability { get; set; }
    public int Count { get; set; }
    public WordChoice(string leading, string next, float probability, int count)
    {
        LeadingWord = leading;
        NextWord = next;
        Probability = probability;
        Count = count;
    }
}
