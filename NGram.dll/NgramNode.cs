using System;
using System.Collections.Generic;

namespace Troshkin.Ngram;

public struct NgramNode
{
  public string Ngram { get; private set; }
  public int Count { get; set; }
  public NgramNode(string ngram, int count)
  {
    Ngram = ngram;
    Count = count;
  }
}
