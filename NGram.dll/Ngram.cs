using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Troshkin.Ngram;
public static class Ngram
{
  /// <summary>
  /// Формирует коллекцию N-gram
  /// </summary>
  /// <param name="sr">Text stream for reading</param>
  /// <param name="n">N in N-gram</param>
  /// <returns></returns>
  public static async Task<IEnumerable<NgramNode>> GetMostFreqNgramsAsync(
    StreamReader sr,
    int n,
    int k,
    IProgress<int>? progress = null,
    CancellationToken cancelToken = default)
  {
    string? line;
    //потокобезопасная коллекция для NGram
    var ngrams = new ConcurrentDictionary<string, int>();
    while ((line = await sr.ReadLineAsync()) is not null)
    {
      var lineParts = line.Split(new[] { '.', ',', '!', '?', ';', ':', '—' }, StringSplitOptions.RemoveEmptyEntries);
      ThreadPool.GetMaxThreads(out int workerThreads, out _);
      Parallel.ForEach(
        lineParts,
        new ParallelOptions { CancellationToken = cancelToken, MaxDegreeOfParallelism = workerThreads },
        part =>
        {
          foreach (var ngram in GetNgrams(part, n))
          {
            //https://medium.com/gft-engineering/correctly-using-concurrentdictionarys-addorupdate-method-94b7b41719d6
            ngrams.AddOrUpdate(ngram, 1, (k, v) => ++v);
          }
        });
      progress?.Report(1);
    }
    checked
    {
      return ngrams.Select(ng => new NgramNode(ng.Key, ng.Value))
        .OrderByDescending(ng => ng.Count)
        .Take(k);
    }
  }
  public static async Task<IEnumerable<NgramNode>> GetMostFreqNgramsWithoutParallelAsync(
    StreamReader sr,
    int n,
    int k)
  {
    string? line;
    //Не ConcurrentDictionary<string, int>, потому что AddOrUpdate непотокобезопасен Func 
    var ngrams = new List<string>();
    while ((line = await sr.ReadLineAsync()) is not null)
    {
      var lineParts = line.Split(new[] { '.', ',', '!', '?', ';', ':', '—' }, StringSplitOptions.RemoveEmptyEntries);
      ThreadPool.GetMaxThreads(out int workerThreads, out _);
      foreach (var part in lineParts)
      {
        foreach (var ngram in GetNgrams(part, n))
        {
          ngrams.Add(ngram);
        }
      }
    }
    checked
    {
      return ngrams.GroupBy(ng => ng).Select(g => new NgramNode(g.Key, g.Count()))
        .OrderByDescending(ng => ng.Count)
        .Take(k);
    }
  }
  public static async Task<IEnumerable<NgramNode>> GetMostFreqBigramsAsync(
  StreamReader sr,
  int k,
  IProgress<int>? progress = null,
  CancellationToken cancelToken = default)
  {
    return await GetMostFreqNgramsAsync(sr, 2, k, progress, cancelToken);
  }
  public static async Task<IEnumerable<NgramNode>> GetMostFreqTrigramsAsync(
    StreamReader sr,
    int k,
    IProgress<int>? progress = null,
    CancellationToken cancelToken = default)
  {
    return await GetMostFreqNgramsAsync(sr, 3, k, progress, cancelToken);
  }

  private static IEnumerable<string> GetNgrams(string str, int n)
  {
    var charList = new LinkedList<char>();
    foreach (var ch in str)
    {
      //Для исключения пунктуации, разделителей эмоджи и т.д.
      if (Char.IsLetter(ch))
      {
        charList.AddLast(ch);
        //Если ngram набралась, то добавляю её в коллекцию
        if (charList.Count == n)
        {
          var completeNgram = new String(charList.ToArray());
          //Удаляю первый элемент коллекции(sliding window)
          charList.RemoveFirst();
          yield return completeNgram;
        }
      }
      //Встретился не буквенный символ, сбрасываю коллекцию
      else
      {
        charList.Clear();
      }
    }
  }
}
