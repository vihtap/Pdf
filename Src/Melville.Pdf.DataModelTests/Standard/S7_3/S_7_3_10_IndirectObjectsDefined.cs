﻿using System.Threading.Tasks;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S_7_3_10_IndirectObjectsDefined
{
    [Fact]
    public async Task ParseReferenceAsync()
    {
        #warning -- find a better way to test this -- also note that 7.3.10 says that a top-level object out side of an object stream can be a reference to an other object -- need to follow the stream
        // var src = "[24 543 R]".AsParsingSource();
        // src.NewIndirectResolver.RegisterDirectObject(24, 543, 12345);
        // var result = (await (await src.ParseValueObjectAsync()).LoadValueAsync())
        //     .Get<PdfValueArray>();
        //
        // var item = await result[0];
        // Assert.Equal(12345, item.Get<long>());            
    }

}