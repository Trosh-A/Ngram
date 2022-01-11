using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Troshkin.Ngram.ConsoleApp
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Начало работы программы");
      var timer = Stopwatch.StartNew();
      if (args.Length == 0)
      {
        Console.WriteLine("Не задан путь к файлу");
        Console.ReadKey();
        return;
      }
      string pathToFile = args[0];
      try
      {
        var progressIndicator = new Progress<long>(prog => Debug.WriteLine($"Завершена строка {prog}"));
        var concreteTimer = Stopwatch.StartNew();
        var allResults = await Ngram.GetNgramAsync(pathToFile, 3, progressIndicator);
        timer.Stop();
        if (allResults is null)
        {
          Console.WriteLine("Ошибка. Операция не выполнена.");
        }
        else
        {
          PrintResults(allResults.Take(10));
          Console.WriteLine($"Время работы задачи по поиску триплетов: {concreteTimer.ElapsedMilliseconds} мс");
          Console.WriteLine($"Время работы всей программы {timer.ElapsedMilliseconds} мс");
        }
      }
      catch (FileNotFoundException)
      {
        Console.WriteLine("Файл не найден");
        Console.ReadKey();
        return;
      }
      catch (PathTooLongException)
      {
        Console.WriteLine("Слишком длинный путь к файлу");
      }
      catch (DirectoryNotFoundException)
      {
        Console.WriteLine("Папка не найдена");
      }
      catch (UnauthorizedAccessException)
      {
        //readonly или не поддерживается средой исполнения или нет прав доступа
        Console.WriteLine("Нет доступа к файлу");
      }
      catch (NotSupportedException)
      {
        Console.WriteLine("Неверный формат");
      }
      //нет разрешение на чтение файла
      catch (SecurityException)
      {
        Console.WriteLine("Нет разрешения на чтение файла");
      }
      catch (IOException)
      {
        Console.WriteLine("Ошибка подсистемы ввода/вывода");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return;
      }
      finally
      {
        Console.WriteLine("Нажмите любую кнопку для выхода...");
        Console.ReadKey();
      }
    }

    private static void PrintResults(IEnumerable<NgramNode> results)
    {
      if (!results.Any())
      {
        Console.WriteLine("Триплетов в тексте нет");
      }
      else
      {
        Console.WriteLine("Топ 10 самых встречающихся триплетов");
        foreach (var ngram in results)
        {
          Console.WriteLine($"{ngram.Ngram} - {ngram.Count}");
        }
      }
    }
  }
}