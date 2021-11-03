﻿using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class TextObjectOperationsTest : WriterTest
{
    [Fact]
    public async Task EmptyBlock()
    {
        using (var block = sut.StartTextBlock())
        {
        }
        Assert.Equal("BT\nET\n", await WrittenText() );
        
    }

    [Fact]
    public async Task MovePositionBy()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MovePositionBy(3, 4);
        }
        Assert.Equal("BT\n3 4 Td\nET\n", await WrittenText() );
        
    }
    [Fact]
    public async Task MovePositionByWithLeading()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MovePositionByWithLeading(3, 4);
        }
        Assert.Equal("BT\n3 4 TD\nET\n", await WrittenText() );
    }
    [Fact]
    public async Task SetTextMatrix()
    {
        using (var block = sut.StartTextBlock())
        {
            block.SetTextMatrix(1,2,3,4,5,6);
        }
        Assert.Equal("BT\n1 2 3 4 5 6 Tm\nET\n", await WrittenText() );
    }
    [Fact]
    public async Task ToNextLine()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MoveToNextTextLine();
        }
        Assert.Equal("BT\nT*\nET\n", await WrittenText() );
    }
    [Fact]
    public async Task ShowString()
    {
        using (var block = sut.StartTextBlock())
        {
            block.ShowString("ABC");
        }
        Assert.Equal("BT\n(ABC)Tj\nET\n", await WrittenText() );
    }
    [Fact]
    public async Task NextLineAndShowString()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MoveToNextLineAndShowString("ABC");
        }
        Assert.Equal("BT\n(ABC)'\nET\n", await WrittenText() );
    }
    [Fact]
    public async Task NextLineAndShowString2()
    {
        using (var block = sut.StartTextBlock())
        {
            block.MoveToNextLineAndShowString(2, 3, "ABC");
        }
        Assert.Equal("BT\n2 3(ABC)\"\nET\n", await WrittenText() );
    }
}