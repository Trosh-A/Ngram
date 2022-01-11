using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Troshkin.Ngram;
public class Ngram
{
  /// <summary>
  /// Формирует коллекцию N-gram
  /// </summary>
  /// <param name="sr">Text stream for reading</param>
  /// <param name="n">N in N-gram</param>
  /// <returns></returns>
  public static async Task<IEnumerable<NgramNode>> GetNgramAsync(
    string filePath,
    int n,
    IProgress<long>? progress = null,
    CancellationToken cancelToken = default)
  {
    //Потокобезопасная словарь для n-gram
    var ngrams = new ConcurrentBag<string>();
    var lines = await File.ReadAllLinesAsync(filePath, cancelToken);
    var completedLines = 0;
    Parallel.ForEach(
      lines,
      new ParallelOptions { CancellationToken = cancelToken },
      (line, _, i) =>
      {
        var charList = new LinkedList<char>();
        foreach (var ch in line)
        {
          //Для исключения пунктуации, эмоджи и т.д.
          if (Char.IsLetter(ch))
          {
            charList.AddLast(ch);
            //Если ngram набралась, то добавляю её в коллекцию
            if (charList.Count == n)
            {
              var completeNgram = new String(charList.ToArray());
              ngrams.Add(completeNgram);
              //Удаляю первый элемент коллекции(sliding window)
              charList.RemoveFirst();
            }
          }
          //Встретился не буквенный символ, сбрасываю коллекцию
          else
          {
            charList.Clear();
          }
        }
        Interlocked.Increment(ref completedLines);
        progress?.Report(i);
      });
    var result = ngrams.GroupBy(ng => ng).Select(g => new NgramNode(g.Key, g.Count())).ToList();
    return result.OrderByDescending(ng => ng.Count);
  }
}
