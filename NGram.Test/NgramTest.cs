using System.IO;
using Troshkin.Ngram;
using Xunit;

namespace NGram.Test;
public class NgramTest
{
  [Fact]
  public void Test1()
  {
    var pathToFile = "Test1.txt";
    //Arrange
    using var sr = new StreamReader(pathToFile, new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read });

    //Act
    var result = Ngram.GetMostFreqNgramsAsync(sr, 3, 10).GetAwaiter().GetResult();

    //Assert
    Assert.Contains<NgramNode>(new NgramNode("ing", 21), result);
    Assert.Contains<NgramNode>(new NgramNode("con", 10), result);
  }

  [Fact]
  public void Test2()
  {
    var pathToFile = "Test2.txt";
    //Arrange
    using var sr = new StreamReader(pathToFile, new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read });

    //Act
    var result = Ngram.GetMostFreqNgramsAsync(sr, 3, 10).GetAwaiter().GetResult();

    //Assert
    Assert.Contains<NgramNode>(new NgramNode("���", 2352), result);
    Assert.Contains<NgramNode>(new NgramNode("���", 1511), result);
  }
  [Fact]
  public void Test3()
  {
    var pathToFile = "Test3.txt";
    //Arrange
    using var sr = new StreamReader(pathToFile, new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read });

    //Act
    var result = Ngram.GetMostFreqNgramsAsync(sr, 3, 10).GetAwaiter().GetResult();

    //Assert
    Assert.Contains<NgramNode>(new NgramNode("���", 4207), result);
    Assert.Contains<NgramNode>(new NgramNode("���", 4626), result);
    Assert.Contains<NgramNode>(new NgramNode("���", 4165), result);
  }
}