﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

[MacroItem("Hello World.", "GhVa[c,n(/#gY0H8^RV?***28~>", "FlateDecode", "KnownNames.FlateDecode")]
[MacroItem("-----A---B", "(;QS2(`<Y^~>", "LzwDecode", "KnownNames.LZWDecode")]
[MacroCode("public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", new PdfArray(KnownNames.ASCII85Decode, ~3~)){}}")]
public partial class S7_4_4LzwAndFlateDecodeFilter
{

    [Theory]
    [InlineData(10, 1)]
    [InlineData(100, 1)]
    [InlineData(499, 1)]
    [InlineData(10000, 1)]
    [InlineData(10000, 0)]
    [InlineData(10000, 2)]
    public async Task EncodeRandomStream(int length, int EarlySwitch)
    {
        var buffer = new byte[length];
        var rnd = new Random(10);
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte) rnd.Next(256);
        }

        var creator = new LowLevelDocumentBuilder();
        var param =
            new DictionaryBuilder()
                .WithItem(KnownNames.EarlyChange, EarlySwitch)
                .AsDictionary();

        var str = new DictionaryBuilder()
            .WithFilter(FilterName.LZWDecode)
            .WithFilterParam(EarlySwitch < 2 ? param : null)
            .AsStream(buffer);
        var destination = new byte[length];
        var decoded = await str.StreamContentAsync();
        await decoded.FillBufferAsync(destination, 0, length);
        Assert.Equal(buffer.Length, destination.Length);

        for (int i = 0; i < buffer.Length; i++)
        {
            Assert.True(buffer[i] == destination[i], $"Position: {i} Expected: {buffer[i]} got {destination[i]}");
        }
    }

    [Fact]
    public async Task LzwDecodeBug()
    {
        var str = new DictionaryBuilder()
            .WithFilter(FilterName.ASCIIHexDecode, FilterName.LZWDecode)
            .AsStream(LzwDecodeBugText, StreamFormat.DiskRepresentation);
        var result = await new StreamReader(await str.StreamContentAsync()).ReadToEndAsync();
        Assert.Equal(LwzDecodeBugResult.Replace("\r\n","\r"), result);
        
        // assertion is the lack of an exception
    }

    private const string LwzDecodeBugResult = @"0.7 0.7 0.7 RG
0 J 0 j 0.96 w 10 M []0 d
BX /GS1 gs EX
1 i 
302.16 738 m
302.16 36 l
S
BT
/F5 1 Tf
16 0 0 16 39.12 759 Tm
0.935 0.077 0.124 rg
0 Tc
(EMERGENCY)Tj
/F7 1 Tf
11 0 0 11 35.16 18.96 Tm
0 0 0 rg
BX /GS2 gs EX
0 Tw
(8)Tj
ET
0.935 0.077 0.124 RG
BX /GS1 gs EX
36 754.32 m
568.08 754.32 l
S
BT
/F16 1 Tf
11 0 0 11 307.56 729.24 Tm
BX /GS2 gs EX
(j)Tj
/F7 1 Tf
1.6364 0 TD
(If patient is in shock)Tj
/F11 1 Tf
0 -1.2 TD
[(\245)-661(Run in the amount of fluid listed in bo)18(x belo)8(w)64(,)99( as)]TJ
1.0145 -1.2 TD
-0.015 Tw
[(fast as the fluid will flo)7(w \(wide open\).)]TJ
-1.0145 -1.2 TD
0 Tw
[(\245)-661(After fluid has run in,)104( r)19(echeck P)143(,)99( BP)151(,)99( and general)]TJ
1.0145 -1.2 TD
-0.01 Tc
[(appearance)-14(.)]TJ
-1.0145 -1.2 TD
0 Tc
(\245)Tj
/F13 1 Tf
1.0145 0 TD
-0.033 Tw
[(If still in shoc)-21(k:)]TJ
/F11 1 Tf
0 -1.2 TD
0 Tw
[(\320)-733(r)25(epeat same amount of fluid as fast as it will)]TJ
1.2327 -1.2 TD
[(flo)17(w)54(,)99( then r)26(echeck patient.)]TJ
-2.2473 -1.2 TD
(\245)Tj
ET
0 0 0 RG
0.55 w 
364.8 622.54 m
336.72 622.54 l
S
BT
11 0 0 11 336.72 623.64 Tm
[(Repor)-22(t NO)34(W to y)31(our r)17(e)0(f)9(e)0(r)13(ral doctor)98(.)]TJ
T*
[(\320)-733(while waiting to r)25(epor)-15(t,)94( follo)15(w this plan until )]TJ
/F15 1 Tf
19.44 0 TD
(1)Tj
/F11 1 Tf
-18.2073 -1.2 TD
(of these is true:)Tj
/F16 1 Tf
8 0 0 8 350.28 584.04 Tm
(n)Tj
/F11 1 Tf
11 0 0 11 363.72 584.04 Tm
(patient is out of shock.)Tj
/F16 1 Tf
8 0 0 8 350.28 570.84 Tm
(n)Tj
/F11 1 Tf
11 0 0 11 363.72 570.84 Tm
-0.001 Tw
[(doctor giv)18(es diff)14(er)13(ent or)17(ders.)]TJ
-3.4691 -13.2 TD
-0.025 Tw
[(\245)-661(Appl)10(y pneumatic anti-shock garment \(P)82(ASG,)]TJ
1.0145 -1.2 TD
(MAST)Tj
2.5091 0.3164 TD
0 Tw
(\250)Tj
0.7418 -0.3164 TD
[(\).)95( \(See )]TJ
/F13 1 Tf
2.7382 0 TD
[(Using the Pneumatic )83(Antishoc)-16(k)]TJ
-5.9891 -1.2 TD
[(Garment,)97( )]TJ
/F11 1 Tf
3.6982 0 TD
-0.031 Tc
[(p.)68( )]TJ
/F15 1 Tf
0.8618 0 TD
0 Tc
(80)Tj
/F11 1 Tf
1.1127 0 TD
(.\))Tj
-5.6727 -1.2 TD
[(\320)-733(t)0(r)-24(y)0( to contact doctor )]TJ
ET
462.48 384.94 m
449.16 384.94 l
S
BT
11 0 0 11 449.16 386.04 Tm
(bef)Tj
ET
472.8 384.94 m
462.6 384.94 l
S
BT
11 0 0 11 462.6 386.04 Tm
(or)Tj
ET
478.08 384.94 m
472.8 384.94 l
S
BT
11 0 0 11 472.8 386.04 Tm
(e inflating.)Tj
/F16 1 Tf
8 0 0 8 350.28 372.84 Tm
(n)Tj
/F7 1 Tf
11 0 0 11 363.72 372.84 Tm
(Caution:)Tj
/F11 1 Tf
4.0145 0 TD
-0.017 Tw
[( y)23(ou MUST r)16(epor)-15(t to y)19(our r)22(e)0(f)9(e)0(r)13(ral)]TJ
-4.0145 -1.2 TD
(doctor )Tj
ET
410.76 358.54 m
397.44 358.54 l
S
BT
11 0 0 11 397.44 359.64 Tm
(bef)Tj
ET
420.96 358.54 m
410.76 358.54 l
S
BT
11 0 0 11 410.76 359.64 Tm
(or)Tj
ET
426.24 358.54 m
420.96 358.54 l
S
BT
11 0 0 11 420.96 359.64 Tm
0 Tw
[(e inflating,)100( )]TJ
/F13 1 Tf
4.2545 0 TD
(if patient has any of)Tj
-9.4582 -1.2 TD
(these:)Tj
/F16 1 Tf
7 0 0 7 363.72 333.24 Tm
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 333.24 Tm
(chest pain.)Tj
/F16 1 Tf
7 0 0 7 363.72 320.04 Tm
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 320.04 Tm
[(abnormal hear)-20(t rate or rh)41(ythm.)]TJ
/F16 1 Tf
7 0 0 7 363.72 306.84 Tm
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 306.84 Tm
0.001 Tw
[(se)13(v)23(e)0(r)24(e)0( shor)-28(tness of br)28(eath.)]TJ
/F16 1 Tf
7 0 0 7 363.72 293.64 Tm
0 Tw
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 293.64 Tm
[(br)23(eath sounds:)104( crackles.)]TJ
-3.6873 -1.2 TD
[(\320)-733(a)0(l)13(w)0(a)44(ys tr)-36(y IV fluids )]TJ
ET
453.6 279.34 m
436.2 279.34 l
S
BT
11 0 0 11 436.2 280.44 Tm
(first.)Tj
-9.0436 -1.2 TD
(\320)Tj
/F13 1 Tf
1.2327 0 TD
[(if you can NO)81(T r)10(eac)-17(h doctor and m)12(ust inflate suit:)]TJ
/F16 1 Tf
8 0 0 8 350.28 254.04 Tm
(n)Tj
/F11 1 Tf
11 0 0 11 363.72 254.04 Tm
[(inflate legs until blood pr)23(essur)18(e is )]TJ
/F15 1 Tf
13.7891 0 TD
(90)Tj
/F11 1 Tf
1.1127 0 TD
( or)Tj
-14.9018 -1.2 TD
[(v)13(elcr)26(o cracks.)]TJ
/F16 1 Tf
8 0 0 8 350.28 227.64 Tm
(n)Tj
/F11 1 Tf
11 0 0 11 363.72 227.64 Tm
[(do NO)64(T inflate abdominal section until y)24(o)0(u)]TJ
T*
-0.001 Tw
[(talk to doctor)93(.)]TJ
/F16 1 Tf
8 0 0 8 350.28 201.24 Tm
0 Tw
(n)Tj
/F11 1 Tf
11 0 0 11 363.72 201.24 Tm
[(do NO)64(T inflate abdominal section )]TJ
/F13 1 Tf
14.0509 0 TD
(if patient:)Tj
/F16 1 Tf
7 0 0 7 363.72 188.04 Tm
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 188.04 Tm
(is a child.)Tj
/F16 1 Tf
7 0 0 7 363.72 174.84 Tm
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 174.84 Tm
[(is mor)26(e than )]TJ
/F15 1 Tf
5.3673 0 TD
(20)Tj
/F11 1 Tf
1.1127 0 TD
[( w)15(eeks pr)28(egnant.)]TJ
/F16 1 Tf
7 0 0 7 363.72 161.64 Tm
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 161.64 Tm
(has abdominal organs coming out of a)Tj
0 -1.2 TD
-0.002 Tw
[(w)21(ound \(e)20(visceration\).)]TJ
/F16 1 Tf
7 0 0 7 363.72 135.24 Tm
0 Tw
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 135.24 Tm
[(has a f)13(o)0(r)21(eign body \(impaled object\))]TJ
T*
-0.001 Tw
(sticking into abdomen.)Tj
/F16 1 Tf
7 0 0 7 363.72 108.84 Tm
0 Tw
(1)Tj
/F11 1 Tf
11 0 0 11 377.28 108.84 Tm
[(has uncontr)30(olled bleeding in an ar)22(ea that)]TJ
T*
-0.014 Tw
[(will be outside the suit:)105( chest,)98( arms,)105( scalp)29(,)]TJ
T*
0 Tw
[(face)-21(,)99( or neck.)]TJ
/F7 1 Tf
-26.6836 2.9564 TD
-0.001 Tw
[(Ag)14(e in )171(Y)130(ears)-3202(Dr)28(ops/Min)12(ute)]TJ
/F11 1 Tf
0.5127 -1.3091 TD
0.001 Tw
(Less than)Tj
/F15 1 Tf
3.7745 0 TD
6.182 Tc
-6.181 Tw
[( 14)]TJ
/F11 1 Tf
7.5709 0 TD
0 Tc
0 Tw
(-)Tj
/F15 1 Tf
0.3273 0 TD
(6)Tj
-10.0909 -1.3091 TD
[(1-2)-8214(8)]TJ
T*
[(3-7)-7931(10)]TJ
-0.2727 -1.3091 TD
[(8-11)-7649(16)]TJ
-1.3855 -1.3091 TD
(12 )Tj
/F11 1 Tf
1.3855 0 TD
0.001 Tw
(or older)Tj
/F15 1 Tf
9.6436 0 TD
0.001 Tc
(20)Tj
ET
0.48 w 
BX /GS1 gs EX
64.08 111.36 m
266.28 111.36 l
S
0.8 0.8 0.8 RG
63.96 97.92 m
265.92 97.92 l
63.96 84 m
266.4 84 l
63.96 69.24 m
266.28 69.24 l
63.96 54.36 m
266.28 54.36 l
S
0 0 0 RG
1000 M 
63.99 125.16 202.32 -84.27 re
S
10 M 
164.64 125.28 m
164.64 40.8 l
S
BT
/F7 1 Tf
11 0 0 11 337.44 542.88 Tm
BX /GS2 gs EX
0 Tc
0 Tw
[(IV FLUID FOR P)106(A)126(TIENTS IN SHOCK)]TJ
1.0145 -1.3091 TD
[(Ag)14(e in )183(Y)130(ears)-1740(Fluid )103(Amount in ML)]TJ
/F11 1 Tf
0.5018 -1.3091 TD
0.001 Tw
(Less than )Tj
/F15 1 Tf
4.0582 0 TD
[(1)-5075(100-175)]TJ
-2.4655 -1.3091 TD
0 Tw
(1)Tj
/F11 1 Tf
0.5564 0 TD
(-)Tj
/F15 1 Tf
0.3164 0 TD
[(2)-7661(250)]TJ
-0.8727 -1.3091 TD
(3)Tj
/F11 1 Tf
0.5564 0 TD
(-)Tj
/F15 1 Tf
0.3164 0 TD
[(7)-7661(400)]TJ
-1.1564 -1.3091 TD
(8)Tj
/F11 1 Tf
0.5564 0 TD
(-)Tj
/F15 1 Tf
0.3273 0 TD
[(11)-7379(700)]TJ
-2.2691 -1.3091 TD
(12)Tj
/F11 1 Tf
1.1018 0 TD
0.001 Tw
( or older)Tj
/F15 1 Tf
9.3818 0 TD
-0.001 Tc
(1000)Tj
ET
BX /GS1 gs EX
335.16 525 m
534.6 525 l
S
0.8 0.8 0.8 RG
335.16 511.44 m
534.36 511.44 l
335.16 496.68 m
534 496.68 l
335.16 482.64 m
534.36 482.64 l
335.16 467.76 m
534.36 467.76 l
S
0 0 0 RG
424.2 539.4 m
424.2 453 l
S
1000 M 
335.44 539.41 198.92 -86.42 re
S
BT
/F7 1 Tf
11 0 0 11 36 729.24 Tm
BX /GS2 gs EX
0 Tc
[(Emerg)15(ency-)]TJ
/F9 1 Tf
5.5964 0 TD
0 Tw
(1)Tj
/F7 1 Tf
0.5564 0 TD
-0.001 Tw
[(:)96( Shock)]TJ
/F16 1 Tf
-6.1527 -2.4 TD
0 Tw
(j)Tj
/F7 1 Tf
1.6364 0 TD
-0.001 Tw
[(Begin emerg)18(ency car)17(e)]TJ
/F11 1 Tf
0 -1.2 TD
0.018 Tw
[(\245)-671(Check )96(ABCs:)109( )20( )114(Airwa)39(y)89(,)99( Br)22(eathing,)93( Cir)26(culation.)]TJ
T*
-0.017 Tw
[(\245)-671(Use dir)24(ect pr)18(essur)18(e to contr)25(ol se)20(v)23(e)0(r)13(e)0( bleeding.)]TJ
1.0255 -1.2 TD
(\(See )Tj
/F15 1 Tf
11 0 2.039 11 87.48 663.24 Tm
[(1.)111( )]TJ
/F13 1 Tf
11 0 0 11 98.52 663.24 Tm
0.001 Tw
[(Use Dir)16(ect Pr)6(essur)19(e)-31(,)]TJ
/F11 1 Tf
7.56 0 TD
-0.027 Tc
0.028 Tw
[( p.)72( )]TJ
/F15 1 Tf
1.1564 0 TD
0 Tc
0 Tw
(6)Tj
/F11 1 Tf
0.5564 0 TD
(.\))Tj
-13.32 -1.2 TD
[(\245)-671(H)0(a)32(v)23(e a helper position patient so that he is l)12(ying)]TJ
1.0255 -1.2 TD
-0.001 Tc
(down.)Tj
T*
0 Tc
[(\320)-722(ele)10(vate legs about )]TJ
/F15 1 Tf
8.7164 0 TD
(12)Tj
/F11 1 Tf
1.1127 0 TD
( inches higher than head)Tj
ET
0.55 w 10 M 
92.64 609.34 m
78.72 609.34 l
S
BT
11 0 0 11 78.72 610.44 Tm
(onl)Tj
ET
97.44 609.34 m
92.64 609.34 l
S
BT
11 0 0 11 92.64 610.44 Tm
-0.011 Tw
[(y if patient has no injuries to legs,)105( pelvis,)97( hips,)]TJ
-1.2655 -1.2 TD
-0.004 Tc
-0.07 Tw
(neck, back, chest, or abdomen.)Tj
-1.2218 -1.2 TD
0 Tc
0 Tw
(\320)Tj
/F13 1 Tf
1.2218 0 TD
-0.013 Tw
[(if patient has head injur)-46(y)]TJ
/F11 1 Tf
9.3164 0 TD
[( or shor)-16(tness of br)23(eath,)]TJ
-9.3164 -1.2 TD
0 Tw
[(it is better for him to lie flat,)112( or e)8(v)23(en to sit up a)]TJ
T*
[(little)-19(.)]TJ
-2.2473 -1.2 TD
[(\245)-671(K)67(eep patient warm,)93( but not hot.)105( Ha)38(v)13(e)0( a helper)-34(:)]TJ
1.0255 -1.2 TD
[(\320)-722(r)14(emo)13(v)23(e)0( w)14(et clothes.)]TJ
T*
[(\320)-722(place blank)26(ets o)9(v)23(er and under patient.)]TJ
/F16 1 Tf
-2.6618 -1.2 TD
(j)Tj
/F7 1 Tf
1.6364 0 TD
-0.002 Tw
(Vital signs)Tj
/F11 1 Tf
0 -1.2 TD
0 Tw
[(\245)-671(Check P)152(,)99( R,)98( BP)151(.)]TJ
/F16 1 Tf
-1.6364 -1.2 TD
(j)Tj
/F7 1 Tf
1.6364 0 TD
[(Decide if tr)17(eatment is needed)]TJ
/F11 1 Tf
0 -1.2 TD
(\245)Tj
/F13 1 Tf
1.0255 0 TD
[(If patient f)16(eels OK)]TJ
/F11 1 Tf
6.8509 0 TD
( )Tj
ET
159.6 464.14 m
143.76 464.14 l
S
BT
11 0 0 11 143.76 465.24 Tm
[(and)8( has no obvious blood or fluid)]TJ
-7.1345 -1.2 TD
(loss:)Tj
T*
[(\320)-722(obser)-33(v)23(e)-23(.)]TJ
T*
[(\320)-722(y)13(ou do NO)63(T ha)42(v)23(e)0( to tr)20(eat f)11(or shock.)]TJ
T*
[(\320)-722(this ma)33(y just be lo)18(w BP with standing \(postural)]TJ
1.2218 -1.2 TD
-0.008 Tc
[(h)34(ypotension\).)]TJ
/F16 1 Tf
8 0 0 8 78.72 386.04 Tm
0 Tc
(n)Tj
/F11 1 Tf
11 0 0 11 92.28 386.04 Tm
-0.015 Tw
[(while waiting to r)25(epor)-26(t,)105( no)6(w g)13(o)0( to )]TJ
/F13 1 Tf
14.0945 0 TD
[(NER)27(V)30(OUS)-29(,)]TJ
-14.0945 -1.2 TD
-0.02 Tw
[(Other Ner)-30(vous System Pr)27(oblems)-36(,)]TJ
/F11 1 Tf
12.2945 0 TD
-0.027 Tc
0.007 Tw
[( p.)83( )]TJ
/F15 1 Tf
1.1018 0 TD
0 Tc
(452)Tj
/F11 1 Tf
1.6691 0 TD
0 Tw
(.)Tj
-18.5455 -1.2 TD
[(\245)-671(F)11(or other patients,)111( continue to f)19(ollow this plan.)]TJ
/F16 1 Tf
-1.6364 -1.2 TD
(j)Tj
/F7 1 Tf
1.6364 0 TD
[(Repor)-15(t)]TJ
/F11 1 Tf
3.2836 0 TD
-0.001 Tw
[(.)99( Ha)37(v)23(e)0( someone contact y)22(our r)16(e)0(f)9(e)0(r)13(ral doctor)]TJ
-3.2836 -1.2 TD
0 Tw
[(and ar)7(range f)19(or transpor)-31(t to hospital,)110( while y)21(o)0(u)]TJ
T*
0.001 Tw
[(f)10(ollow this plan.)]TJ
/F16 1 Tf
-1.6364 -1.2 TD
0 Tw
(j)Tj
/F7 1 Tf
1.6364 0 TD
[(Giv)23(e o)28(xyg)11(en)]TJ
/F11 1 Tf
0 -1.2 TD
[(\245)-671(Set the flo)14(w rate based on ho)6(w m)23(uch y)16(ou ha)32(v)23(e)]TJ
1.0255 -1.2 TD
[(a)34(vailable)-17(.)]TJ
T*
(\320)Tj
/F15 1 Tf
1.2218 0 TD
(2)Tj
/F11 1 Tf
0.5564 0 TD
(-)Tj
/F15 1 Tf
0.3273 0 TD
(6)Tj
/F11 1 Tf
0.5564 0 TD
[( liters b)10(y)0( nasal cann)16(ula.)]TJ
-2.6618 -1.2 TD
(\320)Tj
/F15 1 Tf
1.2218 0 TD
(12)Tj
/F11 1 Tf
1.1127 0 TD
(-)Tj
/F15 1 Tf
0.3273 0 TD
(15)Tj
/F11 1 Tf
1.1018 0 TD
[( liters b)11(y)0( non-r)20(ebr)11(eathing mask.)]TJ
/F16 1 Tf
-6.4255 -1.2 TD
(j)Tj
/F7 1 Tf
1.6364 0 TD
0.001 Tw
[(Star)-18(t an IV)]TJ
/F11 1 Tf
0 -1.2 TD
-0.01 Tc
[(\245)-681(See )]TJ
/F13 1 Tf
2.6618 0 TD
-0.012 Tw
[(MEDICINE,)95( Star)-30(ting and Giving IV Fluids)-34(,)]TJ
/F11 1 Tf
14.88 0 TD
-0.036 Tc
0.014 Tw
[( p.)74( R)-26(-)]TJ
/F15 1 Tf
1.9855 0 TD
0 Tc
(81)Tj
/F11 1 Tf
1.1127 0 TD
0 Tw
(.)Tj
-20.64 -1.2 TD
[(\245)-671(Use LA)40(CT)101(A)100(TED RINGER\325S or )]TJ
/F15 1 Tf
13.56 0 TD
(0)Tj
/F11 1 Tf
0.5564 0 TD
(.)Tj
/F15 1 Tf
0.2182 0 TD
(9)Tj
/F11 1 Tf
0.5564 0 TD
(% SODIUM)Tj
-13.8655 -1.2 TD
(CHLORIDE solution.)Tj
T*
[(\320)-722(run at maintenance rate unless patient is in)]TJ
1.2218 -1.2 TD
[(shock)-307(or doctor or)17(ders a faster rate)-15(.)]TJ
/F7 1 Tf
0.24 -2.4 TD
-0.001 Tw
[(MAINTENANCE RA)119(TES USING)]TJ
/F9 1 Tf
2.1055 -1.2 TD
(10)Tj
/F7 1 Tf
1.1127 0 TD
[( DR)10(OPS/ML )156(TUBING)]TJ
ET
";

    private static readonly string LzwDecodeBugText = @"
        80 0C 05 C3 71 04 0A 09 06 10 14 88 E0 D1 80 80 95 05 10 1A A0 A2 E1 C8 D8 40 77 10 0C 61 A4 D1 
        01 6C BB 0D 32 03 48 45 81 00 BC 8E 53 18 88 0C E7 31 01 14 B0 0D 94 9A 44 00 D1 98 C0 64 2E 18 
        C5 86 E3 31 C0 80 DB 34 9B 4E 22 C3 38 B1 B0 1A 53 91 15 01 A2 F2 30 D6 32 20 2A 19 A6 11 68 6C 
        36 72 20 19 8E 67 03 21 00 DC 6A 39 A8 4F E0 43 91 9D 3A 04 30 1B C1 EB 63 41 01 C8 CF 0C A8 18 
        C1 A2 82 29 34 8B 0A 22 93 88 65 91 49 50 D5 4B 23 41 25 35 19 84 A6 AB 19 94 D9 68 51 91 C4 52 
        2C 54 B1 44 21 B6 E9 14 92 4C 53 AE 4A E5 B2 F8 69 50 EF 73 1C 5F 6F E4 5A 55 8E CB 13 B4 5A 86 
        23 2B 64 2B 2B 25 93 CA 73 52 E9 A4 E8 6A 34 17 0C EB 93 F1 A8 DB 1A 30 9E D7 B7 1B A1 05 1A 90 
        42 A5 53 2A F8 3A 90 C7 0D 10 E7 56 2D 02 ED ED 74 65 5A D6 58 75 F9 7C CC B3 68 28 35 68 B0 18 
        2A 87 34 5C 36 A2 5B 33 A4 4B 99 24 CC 20 38 18 4E 86 93 29 B8 E8 20 34 CB 0D 26 E1 01 CC D0 37 
        8C 63 5B C4 E5 25 2E 62 E0 16 86 21 72 B8 2A 3D 82 D8 50 2E 35 81 A8 52 16 86 C1 B0 62 14 0A 43 
        AB F8 FD 84 03 A0 D0 32 84 03 08 DA 37 C3 4F B8 DE F7 8C C3 60 EA 34 8C 8E 2B F2 3A 0C B1 6C 38 
        31 0D E1 48 62 1C 05 03 C0 40 31 0C A3 64 6B 1C 0E E1 48 6C 1A 05 01 60 52 1C 87 21 44 42 39 85 
        22 E8 A8 25 26 01 70 60 18 86 8A 74 11 05 2A 0F 60 5A B3 86 2A 73 3C 06 C1 C3 30 C2 39 BE F3 1C 
        3B 0F 84 11 4C 57 16 8E E3 48 D8 36 4D 31 F0 52 1B 85 08 C0 B9 3A C5 91 00 DE 38 3E A2 E0 52 17 
        49 B2 78 1B 2B CA 72 A8 41 2B C1 6F 63 3A CF C1 D0 84 AB 09 C2 B0 B8 82 33 46 03 94 E3 35 84 03 
        44 CC 39 43 4F C0 DD 23 23 52 22 DB 1B 49 23 28 C7 0F C0 21 00 A1 1B 06 81 9C 8B 23 C9 21 00 85 
        55 CB B0 BC 8D 24 49 43 08 DD 16 8C EF A8 CA 39 0C 23 65 03 28 41 34 2C AD 04 D1 34 1C B8 B8 CC 
        01 40 C2 38 4F 83 0D 80 37 0C 63 2C 27 2A 05 14 04 9D 28 50 92 A5 8F 2C 41 8B 80 A8 B9 41 F0 8C 
        06 23 06 21 9A 9E C2 58 B6 F2 21 70 CB 72 90 67 75 4B F0 73 DC FE BE 73 7D 3C FE BF E3 1C 26 19 
        42 E3 58 75 61 B0 0E 8C 0C 86 D1 12 CD C5 46 41 ED D0 61 09 A7 75 70 E4 14 86 41 A8 50 32 DA 2F 
        B8 E7 11 44 11 14 48 FB 04 11 3D 2F 16 49 73 4C C7 32 BF 4F BC DB 37 E0 96 43 74 82 61 37 0C C3 
        39 06 33 A4 82 DB D5 F5 C4 CE FA D4 61 90 6D 8B D4 F5 30 D6 F8 3E 4F A3 ED 6D 50 41 6A 6E D6 27 
        74 3D 91 85 5C B4 7A FC 06 B4 8B 83 0E D7 20 41 AA 9C 8C 36 AD C2 7A 1B 06 49 BB 6E 9F 26 8A 22 
        06 AE 6C 1B 12 D8 E3 29 2C 2B 24 C4 2B 1B 30 6F B4 06 41 9B CE B6 32 16 68 A5 8C 0D F8 9E 94 19 
        05 0F B8 9C 27 85 21 9C 88 2B C3 A3 78 40 3C F0 B0 BC 49 4B 62 79 AE 2E 14 86 01 40 CD 23 F2 7C 
        AF 23 89 58 21 00 C9 00 0E 9B EC 8F 1C 69 12 80 A8 15 59 A2 E6 1D 88 5E 73 A8 D1 37 44 03 B8 C2 
        34 BE 63 70 CF C4 E7 B8 B6 F9 BF 4B BC 0D 6F 51 0C C3 7C DF 1A F7 A8 C4 3C FC BE 03 65 74 10 44 
        B3 70 41 82 39 4A 74 0C 18 AB 41 A3 D5 A8 06 37 3E 0C F2 D0 71 BC 14 B4 5D 59 8B D8 14 64 50 F0 
        CA 39 C4 1E 40 E9 4E 0C B8 1E A6 E5 22 D0 32 7A C3 A7 AB 2A 04 19 27 A1 A8 70 DC 06 1B C2 7E 0A 
        03 73 DA 40 AF 70 E8 98 73 A2 51 1B B3 73 04 0F E9 FE 3F E2 E6 7C 4F 99 F5 3E EF 21 12 22 63 DE 
        7F 90 00 6B 50 0F BD 74 3F 17 B8 FC C8 83 F5 06 AF DD FC 83 72 04 FE CE D4 00 80 4B AC E6 9C F2 
        AC 62 4F 43 67 81 70 98 17 42 86 F2 BC 41 82 53 2A 0C 31 CF 86 37 42 A5 83 38 69 0E C8 D9 1C 3E 
        77 3C 1A 43 33 97 5B 0A FD 1B 2A E8 24 C8 5C 8A 74 0C 8A FC 39 BA 55 06 DD 81 A0 36 07 24 A5 04 
        37 65 93 0E 18 AC 3B 75 2B 99 0A 21 60 50 10 56 82 C2 23 40 A0 3C 9F 00 DC 19 43 A8 6D 68 A1 8D 
        10 9F 60 D2 0B 60 C2 A9 0C EB 48 36 C4 F4 EE AA C1 C3 80 08 21 4C 23 A4 65 B6 94 56 33 4E 5C 0F 
        8C 26 C8 30 A8 78 9B 10 30 8B 64 4C 19 93 96 F0 A2 A3 12 E5 84 67 88 83 03 44 6E A1 C8 14 95 48 
        6C 29 46 A7 F4 8E C5 81 02 77 0A 61 95 10 3D 05 D0 BA 90 31 37 27 72 05 77 A0 D0 50 15 43 99 FB 
        76 EF 99 55 46 F8 E3 1C DE 78 38 55 C1 06 3B 47 95 AE CF D0 14 87 05 A0 D4 8A 03 89 26 F8 96 68 
        47 8F 90 49 5B A7 47 9F 21 D0 24 2C 26 87 9C 1C CB 43 D6 B2 97 91 83 2E 48 38 38 28 03 7C 92 A5 
        72 5D 9B 50 9D 0B 42 05 C2 67 57 20 38 61 F0 71 ED AE C2 70 6A C8 3B 50 05 C9 F8 F1 4C B3 CE DC 
        D9 83 4F 66 4C 34 19 30 F0 5A C4 5C 0B 94 05 0D F8 D6 46 CA 14 EE 03 18 6F 3E C1 86 1F 39 E7 40 
        DF 66 BA 82 6A B1 64 9B 83 47 EA FE C8 A1 6C 27 EF 58 AD 15 72 78 6E 01 CB 6B 28 ED B6 02 9D 02 
        53 49 0C 59 3C 06 C9 4A 07 02 84 78 E5 DA 9D 1B 6E 70 D4 AC 52 0A 52 D9 28 E1 E7 A7 B4 A2 95 1C 
        72 95 4B 61 78 20 A8 25 0C 1C 53 37 FB 0A 5D 15 39 29 40 D0 1B 9B FA 3F 51 2A 05 3B AA F4 84 E2 
        D2 B3 90 DB A0 35 2F AB 54 F6 A7 53 57 D2 1B 91 4B 45 76 D0 6C BF BF 09 B5 08 08 6C 22 84 85 62 
        AD 53 58 03 07 0F 22 EC 85 CD C2 04 43 20 67 5D 61 48 43 0C 21 D4 F9 D1 27 DD 5B 57 44 03 30 8F 
        F1 77 4D F8 70 CD 63 12 0E 71 6C 51 57 22 40 40 13 42 A8 53 0A 8A 8C 9C B1 70 E0 E8 90 43 16 3E 
        EE 86 C9 BD 47 C8 1D 5C 83 14 70 0B 59 CA B9 75 4B 42 9C DD 0B 58 2C 10 16 D8 C5 0D 34 01 44 3D 
        87 EF 3E A9 00 D9 3C 41 8A 1B FA 3A 94 88 9A 03 90 6E 0B 9E B1 58 B8 4D 8D B6 55 FA 91 5F 2E 35 
        C8 2D 85 94 AD 4A 36 F3 4D 83 2D 38 34 75 4E 82 98 EB 94 63 5B 1D 23 23 44 0E E0 DE 0A 8B 4B 2B 
        D9 D1 B7 F7 92 E5 5D 5A 6B 54 6E DD BE 67 C8 2A E9 DC BB 88 0D 2E E9 15 BB F7 0E AE D4 6A C1 4B 
        AA 55 F9 B8 37 B8 ED 28 B5 9B 59 EB 4B B5 0C EA 82 1C CE 89 B1 2B E6 D1 B8 62 AA 1A 6F 82 88 90 
        D1 20 8B 20 53 44 B1 5D 46 D4 4F 3F 5E AB FA 2B 96 E1 F3 3E 8B 0E C1 60 F1 84 9F 24 34 82 57 D8 
        14 BC E2 F5 35 7B 33 D2 C5 42 D6 DF 01 CB 4A 0A 7E AB CE FA 42 96 82 99 1A 21 FB AD 98 A2 6D 62 
        B2 BA 56 21 8E 2F BB B5 3E EB E3 3B 11 3D 71 B5 61 AE 97 1D FC 15 8C 97 03 90 70 61 0C 41 B9 BE 
        C7 24 E0 87 D6 92 FF 72 A7 DD 60 23 08 A0 5B 43 40 29 93 D1 B1 0F 06 D8 AD 5B 90 36 46 C5 B9 24 
        AE 13 5A 67 0D 9F FE 4E 60 B8 D6 FF D4 9A FF 95 1F A8 30 CE F0 38 B3 C3 A5 EA 0A 1F 44 4D 05 11 
        0D BA B9 9A 16 C5 12 25 AC 49 50 62 86 23 80 E9 1B C3 99 2C 64 41 89 89 BF 86 2E 7C 83 46 6F 83 
        B9 15 B7 E7 38 12 57 0E BB 76 BA C6 45 2F 82 8C F5 36 5E 9D E9 31 38 E7 2A EA 96 EE 76 90 76 9C 
        B2 9A 7D 0F 1F D6 3E 19 03 9B 03 54 29 28 31 AC 04 02 1B 1F 3C 56 05 BA A8 1C 34 D9 A0 A3 5D 5D 
        07 75 A1 86 85 46 A5 5C 90 5C AE D4 7A D1 B0 96 3E B4 26 51 23 60 20 09 21 59 91 EC 0A 32 94 28 
        D8 35 D5 40 80 19 03 72 B4 E1 AA 03 66 D5 1B B8 DC DE 7B 9D AC AA 56 F2 DD 93 CA E9 42 90 CC 1A 
        43 92 64 C8 60 B4 AD 3F D2 89 22 96 4D 04 9E 79 3E 58 40 44 14 CB E5 AA CD C2 E1 E6 CB 06 37 96 
        E0 C1 48 38 42 F6 6D C8 B9 50 CB 45 16 BA 74 0D 14 5A 1F 51 85 74 8B 43 6A 36 70 01 D7 20 1F BC 
        12 88 03 9A 2B 0E 8C 0F 07 9C B8 3E DB EB 96 3A DD 86 DE 9A 42 9A EF 93 F3 E5 CF 80 F9 D3 9D C0 
        DD 71 85 AB 43 CA CC DB 20 CD 3C D4 E0 18 91 F0 6F 45 A1 C3 4E C4 ED 33 6A 22 23 17 3F 04 B2 74 
        BD 28 09 02 66 71 CF 5C 20 A0 1C F0 BC F7 36 90 49 CE DD BC 45 25 5F 07 BB 4A 12 99 3D D9 FA 2F 
        45 23 DD 8A C5 19 FB 8A D8 B4 50 35 C5 59 D3 8A 4A 95 70 04 1C E7 5A EE DD 6F 75 F9 FF 67 D6 38 
        DE 18 6A 7D D9 E1 B5 5A CD 73 E0 83 8C 24 30 51 66 F9 77 4A 63 A1 89 CF 86 D3 F6 E7 5F 44 3E 0D 
        34 49 E6 47 64 E0 E3 28 6A 35 72 A1 D5 82 3A 79 C1 0E 4C 1B 0C 0E 8B 05 A1 DA 4B 74 E8 8B 22 D9 
        EF F5 BF 9C 5C AA E7 41 56 46 84 93 3E 27 58 40 4D F1 8B B5 42 53 C7 AD E5 07 79 3F 2A 91 3C C7 
        49 3E 5E 6F CE F9 F4 E1 E8 6C 29 FC 9D 3C 35 76 3F C8 46 58 30 AE 17 82 0D 1B 99 BD AF 00 03 73 
        96 48 F1 C8 DC DF E3 28 57 E2 F2 96 7F E7 5F B3 9E DD 77 90 18 41 02 A7 4D C1 93 21 E7 07 B8 FD 
        2F 90 23 20 6E 6B CF DC C6 8E D0 F8 ED 68 27 A6 6B 00 8E 8E 79 04 46 D3 A6 7E 44 04 3C 79 6E BA 
        9B 49 98 28 86 9A C2 AA 0A FD EE 1C ED 49 F2 A0 62 2E 46 C7 74 0C AE FA 3E 0D 3A 88 A0 CE 0D C5 
        74 0E 8D 44 E6 CC 54 D4 AF D4 86 42 72 41 2F 22 D5 D0 38 AF 4F 18 CA 6F E6 42 CF 0E 7F EC 36 44 
        2F 38 0D EF 3D 05 44 E0 6F A8 F6 0D C2 58 A2 30 86 76 E8 2A 64 23 DE DA 86 A6 61 0A 04 4B 4D 0A 
        41 66 18 48 26 00 B4 E5 76 95 27 26 A0 AD 16 3F 25 AA CC AF 46 0D C4 FD 05 AF CF 00 0E 88 5D 29 
        98 3B 26 F2 C0 D0 6D 00 CF E0 C0 0F E4 CA B0 D4 F9 62 7E 41 D0 7E FF 08 94 B2 AB 60 62 84 2E 0C 
        A0 D3 05 24 76 EA 48 DA 4E E0 D2 0D A3 E2 D9 04 5A 0D E0 C4 0D 45 4C 0E 89 F8 90 EF 5C 87 0D 0C 
        33 ED 10 3E 64 02 97 45 3C B4 8C B4 F3 A3 EA FF AD 46 CE 30 61 00 22 34 31 AC F0 61 62 E6 D5 EB 
        13 00 F0 73 0E 70 14 38 08 6A CB 00 51 07 E4 34 A2 23 EC 62 62 6A 7C 84 DE 46 24 76 D9 04 63 13 
        44 38 79 6C C2 6C 2D 3E 4C E3 E4 F5 A7 50 B1 E6 F1 0A E4 DC E9 E4 F4 B0 89 72 8A 66 76 3F AE 64 
        D8 40 60 95 0C 7E 0E 85 6E 47 04 42 0E 40 DA 0E 6C 18 95 00 E6 E2 C0 D8 0E 06 28 49 29 0C 50 4F 
        5C C0 C4 C2 A2 85 AC 69 45 6C 56 0E DA 52 C8 DE 40 2D 44 AF 22 A4 69 4A 66 37 CE 0E 26 E0 72 37 
        A9 2E F5 F1 2A 59 A0 82 0C E5 58 EB 44 38 46 C0 6E 42 E2 F8 5D 2E 3C 5A 44 98 D9 6A 0A 70 00 89 
        04 E7 C8 0E 00 E6 05 E0 9A 3F 6E 54 05 0B 08 5A CE 6A CF 86 B2 9F 09 14 26 A9 26 9D E4 A5 20 60 
        50 09 87 CE DB A5 34 F8 85 D0 EB C3 08 81 30 06 2C C6 14 A6 64 6E 41 62 E4 42 82 70 E3 4B 22 49 
        44 A8 9D 2C F8 B8 E0 6A 84 CF C0 61 49 E1 15 00 50 05 A7 B5 26 42 A4 94 2D DA 5D 4C 2A 06 C9 FA 
        BC 69 24 92 52 4A 92 43 06 96 C0 62 69 44 26 90 25 B0 34 31 24 75 04 1C 06 6A 0E 62 02 C8 42 E2 
        34 B6 87 EE A0 12 B3 24 E9 6C 07 04 10 7B 2A 0E 48 64 92 27 2B 68 41 22 78 6B 52 DF 2B 71 52 2B 
        90 6E 3C D2 F7 26 D2 50 F6 09 32 A3 07 84 8A 66 26 9E 92 9C 01 AB DC E0 E9 BE D0 A9 C4 2E 70 36 
        B7 A2 04 A3 C2 2E 26 62 46 36 02 50 25 43 BC 25 E4 86 4A 50 14 39 C3 72 22 C2 7E 67 CA 67 0E 93 
        48 E0 E6 D8 84 E2 26 7E 6A 78 35 C8 62 BF 4B A2 07 23 76 01 A6 7C 99 93 6C 04 13 6A 2B 82 8D 36 
        62 2C 85 13 4E 42 AB 90 F0 6A 55 38 00 40 8B 4C 7B 38 73 52 6B E3 B1 38 E6 EC BF 4E 78 E0 F3 98 
        E7 53 A6 28 AA 56 30 E2 1A 35 C2 34 87 2B 2E 26 73 66 2C 03 56 99 82 AF 22 63 72 C4 6A 40 ED 60 
        E4 0C AA 56 23 53 BE 2A 63 70 94 73 C6 E7 42 7E 92 CD 6E 06 93 5C B9 A3 92 30 31 58 FE 2A FE DF 
        A3 6E 26 E0 70 27 A6 F3 33 63 B9 33 C3 36 5C 42 E5 1D 80 50 DC 60 40 08 C0 98 0A A0 92 08 94 1E 
        09 E0 A4 55 44 6C D0 48 CE E5 46 7E 0A 80 92 2F 20 A8 0A 6D C4 09 C0 40 0A 60 90 09 E0 86 09 66 
        5A 4A 45 DC 4A F2 4D 2F C4 1D 20 B2 0E AC E7 9E 46 E5 5D 21 B1 72 E3 EE 02 E4 33 F0 05 00 8C 45 
        46 49 43 29 86 44 64 4A 5F 60 9A 09 92 84 9D 63 A8 EE 32 FB 29 12 52 F6 22 E7 25 8D 33 18 CF B5 
        31 8C 22 4A 4C 44 E2 24 1D 2E 88 46 2B CD 5C 87 24 10 2B CB 68 A3 A0 6D 2F 94 5D 2B 52 90 93 31 
        54 CA 02 18 3A 92 00 ED 92 99 4A C6 0E 37 29 2D 4B 40 50 06 46 20 52 34 EE 93 69 94 84 F2 DD 4C 
        F2 E0 2E 60 67 30 34 DA 6B 49 46 C2 B4 E2 C9 F3 1A 94 34 EA 9B E4 1C 06 F4 F2 8C D3 F0 61 E9 94 
        ED 34 DF 50 12 FC 05 03 43 0E 07 B8 6B 34 DF 51 12 9B 49 46 5F 2A 49 48 D5 D2 E8 27 6D DC 05 08 
        4D 52 86 92 69 68 B4 8B 92 F5 4D 0E C6 35 75 08 ED 34 99 32 34 9E 93 2C CF 31 28 99 4E 47 B8 DD 
        EE 34 9D D0 A9 57 25 C7 15 28 72 EC C6 AB 40 C3 63 41 03 68 5E 73 C8 22 C0 6A 8C 23 78 70 CA 85 
        5A 22 9D 35 AA 79 35 D3 5C 35 D5 9C 31 60 6B 34 8B 93 5A 63 87 5A 15 C0 A5 55 BA 2A E0 68 22 A3 
        CE 27 B5 C4 A9 55 D4 37 CA BB 5C E2 2C A3 C2 6E 94 75 C5 34 AA 94 90 2D 6E 28 D5 E4 A9 40 6C B8 
        E0 6F 34 C0 1A DD 55 C7 5F D6 01 3B 02 91 3B 42 12 21 6B F0 C2 48 16 2B 33 8A A4 63 58 4B 04 AA 
        5D 46 D9 3B A2 36 26 75 BA B9 2D D4 7A A4 0A 9B A2 29 3D 0A 66 BF 02 DB 3D 8B FC 29 91 F0 CF AA 
        F8 27 43 AF 0E C3 B6 24 E3 BB 41 32 92 41 C0 8A 8F A2 DD 04 46 2E 5A 80 F3 29 8C 1F 3C 47 B8 99 
        82 BF 50 F4 D2 D5 B1 55 65 35 3D 68 45 E1 32 52 7E 60 62 2A 49 40 A6 5F A9 92 50 4F FC 30 92 78 
        4B AE D6 69 53 8A 9D E9 32 3C 2A F0 ED 03 CE 3D 2E 23 12 94 A0 41 C0 84 0C A8 82 3F 80 CB 66 D2 
        0C 46 F6 72 0C 68 DA E2 C8 A2 72 72 42 9D 6E 11 49 D4 98 D0 E5 1C 42 44 29 21 60 50 08 65 50 68 
        64 8E 67 E0 82 08 40 86 D8 34 32 56 26 28 72 B4 6A 5B 00 82 E0 07 64 70 A4 92 71 89 9C 67 25 62 
        08 4D 3A B5 6D 40 97 45 6E 55 C0 40 08 6E 00 EF 00 50 0C 60 EA E9 50 C6 8A D1 26 4B 82 09 6F 08 
        C9 5F E4 2E 97 04 40 0C 97 3E A1 B1 1F 04 CE B3 25 AE B1 6D 90 24 EF 4A 24 DB C6 2B 17 43 FA 5A 
        D0 BC D1 88 9D 0F A5 D2 D1 D1 7A 95 97 60 AD 74 56 26 D4 CD 0A 62 E6 95 49 58 B7 95 15 3F AD D8 
        5E 53 C4 25 2D 9A B9 06 BE 86 30 D9 0E ED 5C 50 03 9C 42 ED CE 60 AF B8 CA 2C 01 64 15 A3 39 37 
        B8 D0 95 88 61 97 5C 04 00 89 73 EB 3B 76 60 A0 62 70 22 EA E7 22 B5 ED 97 1E 36 E6 CE 23 A8 2A 
        86 14 8C 17 53 41 64 A4 CA AD 0E 3E 05 00 6E 6C 1D 6A 52 63 6B C4 BB 69 09 31 58 B0 DD 2A 95 38 
        30 96 8E 7A EE C6 9F 60 53 2A A6 EC 38 8E E7 6F 25 21 6F 80 90 A1 4D A8 37 4E E8 D1 A6 3A 53 24 
        7A 4F 85 2C B3 E9 72 FB 2C 30 FC 8D 7D 18 C3 EE 4D 07 90 8D 47 00 0F 29 75 79 66 2A 5B E8 BF 69 
        55 8A B7 20 DE 0E E0 DC C8 71 D6 59 8D A1 03 6A 0F 18 84 7A 5A C8 D6 0E CF A6 38 B6 CC C3 84 68 
        B0 97 C6 7A 2A DE 20 75 1A 7B 14 F1 82 A3 CD 03 CE D8 53 CC 7E 53 31 02 43 E5 2D 02 63 F8 CC 00 
        C8 3C 46 AB 53 D3 33 3D C2 39 31 D5 EA 2D 80 6C 92 4D EA 6C 8A AA 86 58 EA DD ED EC A8 ED F1 8F 
        28 14 42 D3 2E BD E0 DC 58 4B 7A BA 2B 93 8F 78 EE 27 F3 6C D6 F9 14 DE 13 F5 65 43 A3 91 A9 47 
        90 4D FA 86 E4 B9 6C 6D C2 FC 46 8A 89 F0 7E CB 85 3C 0D 4B 50 3E 8D BA 71 4E 99 1C 14 32 95 04 
        F8 0D 80 EC 3F 29 AA 49 47 5F 23 11 D2 5B 85 91 4C B8 7D 80 2D 0A 6F 12 76 2C F7 53 12 D1 EA 0D 
        60 58 47 6E F9 98 31 B3 98 2E 4D 08 28 FB 88 B2 AA 41 46 00 EE 57 9D 29 30 DC 75 53 2A E1 96 BC 
        6C 29 3E B1 C4 B8 5E 86 19 93 8C 33 86 A4 CD 8D 59 42 EB 0B 6A 67 E7 19 7F B5 7F 4E 98 20 59 AC 
        CE D2 89 8E 70 2D 30 D3 43 DE D7 58 50 D4 19 66 50 6D DF 4E A9 A1 41 87 68 EB 64 76 0C A0 E8 52 
        A4 D2 A3 07 5E 0D A7 70 0D 83 E8 4E 23 E4 54 03 57 1E 60 40 5A C4 71 78 56 72 77 18 5E 79 80 E0 
        44 31 90 59 BA 0D 9F ED 90 5A E4 92 D9 46 96 AA 87 C3 79 C5 1B 75 76 F9 45 55 FE 62 E6 31 86 28 
        9E 76 51 BD 73 64 94 0C 58 AC CB 98 6A 0D F0 59 95 24 94 09 18 4B A1 EE EA A1 EF F0 43 F1 CA 89 
        8D 96 48 8E 68 50 45 8B 87 B6 EA A0 79 A4 A0 C6 E6 70 07 22 D2 24 46 D1 5A 20 D2 50 42 89 63 EE 
        0C 64 7C C4 B7 4D 2C 8E 14 62 11 88 0E 07 94 5A B1 7A 57 44 04 67 C6 2E 0E 8D 34 73 1A A2 52 CE 
        4E F4 B3 15 A5 46 8E F7 64 0D 6B 05 23 99 C9 16 2E 76 B8 B1 16 53 06 96 C1 9B 12 53 0A C2 E6 0A 
        C7 68 F4 11 03 09 15 09 0A 5A EB 41 98 41 6F 64 2F 6F C6 85 43 16 AD 72 84 94 0A 51 B6 49 45 67 
        04 44 2F 0C C9 B4 4A E3 D0 94 6B 71 AE E3 C7 6B DB 37 83 09 6C 08 85 4C 4F 23 F0 3D ED BC 72 4E 
        3E 0E 99 92 82 62 58 8D E4 62 46 34 92 60 FA 8E 7C 78 40 9E 97 CA 91 1A 8D 51 D4 1A 3D EF C6 89 
        E8 95 02 24 7A 25 80 9F 45 59 CC 30 8C EE FB F8 C3 8D 8A 8E 2B EA 84 8B 23 70 4A 86 C8 4A 88 13 
        5E 73 43 BA 59 22 E8 44 0A 55 AB D8 8B 30 D7 16 4E 4E E3 39 60 4C D9 41 11 99 5A 44 82 58 EA 00 
        DE EA 4C CE 4D 44 58 B6 8B 8E 5D 2B 6F 79 C0 50 47 CD 32 C4 EF 5D 89 3A 95 18 91 18 7D 06 FC 75 
        BA A2 5F E5 5D AB 27 52 DA 3A 96 A1 D7 8A B2 CF 9C 70 83 D0 F2 E5 32 DB 47 01 C0 37 14 B4 97 79 
        21 E3 EE 89 47 1C 52 C8 F2 83 5A 2F BF 5A B8 70 07 8E 25 88 E4 70 A5 5C 8D B9 44 C8 04 78 38 A7 
        88 48 05 64 0A 02 2E 76 8E 46 4C 8E 4F 13 44 EF 85 C0 E8 B5 0B 66 90 E5 91 99 BB 6B 20 54 08 9C 
        71 66 70 A4 88 0F 2B 3E 46 10 91 0C 70 CB AE 0E 6E 7E 82 BA 31 AC 5E A9 AF EA D5 85 C9 26 14 D9 
        BB 53 78 69 6A 3E AC A3 B4 B1 E4 BD 0A E7 5E D9 02 2E 76 6C 14 77 0D 3A 77 4B 3E A1 86 7F 1B 5A 
        6C 04 0C B8 48 44 EA 25 4D 14 F5 44 94 B4 8F B6 ED 07 F8 A5 33 08 96 C0 9C 2E E6 28 4E 80 AC 70 
        A7 2A 09 EB 32 5F F1 D1 2F 3C EF BE 7A EA 8C 12 7E 09 E7 CC 52 C0 9D A8 11 72 0E DB D1 44 A0 F2 
        4C 96 D2 55 4D 3A 4E 91 18 D9 11 BE DB E6 7F 9E AF 8A 5D 86 97 CF 16 C2 2C EE D7 82 52 53 97 B9 
        D4 9C AE 33 73 90 2A 7A 62 71 56 F6 86 5C 84 AB 8B AE 81 B4 15 5E E2 30 DC E0 67 BC 36 F7 9B AE 
        BA 46 51 FB 15 47 C4 6D C3 26 43 D1 BA DD AC DA 15 7C 51 6C 76 A0 EB 77 24 D2 54 91 74 47 C7 8C 
        75 E2 59 AB C5 75 B3 1A E3 AF 5B 39 BE 9B 3D 65 1B 41 AF 75 4A 6F 7C CE 5A EB 45 B6 67 B8 8B C9 
        85 80 16 93 7D A5 9A 50 06 75 A7 07 0A 4E 9C 24 D2 70 84 0C AA 24 44 1D 9A A2 83 EE F5 07 00 71 
        EB 39 02 2A 14 B5 CD 1C B6 25 80 4E 0F 6E 62 69 95 DD 4E 0F 9F 32 7F AD 6C C2 4E 85 A6 57 BD A2 
        B4 CA 30 7D 65 74 0E 7D CB 7F 67 02 77 03 FF E3 5B 02 0D 9A 15 71 40 EF CB E4 40 F5 07 1C A1 4F 
        59 2C 74 DB 30 CD 0E 89 47 2A 78 5D A8 4C E7 91 DB 19 94 E6 AF CF B3 56 C1 E1 CD 5B DB F3 F9 D6 
        1B 43 4E C0 8E 88 4D 76 4F 46 28 47 00 F0 0F 36 D7 0F E8 03 B8 F2 9F C7 BD 83 6F 57 58 05 09 56 
        B4 64 D0 45 27 88 48 82 30 CC A4 40 0C 44 C7 17 8F 48 3F FC DC 23 0E 52 D1 A0 EA 54 EB 4A EF 20 
        EB C1 E7 0B C2 3E 8B 87 9D 7E 59 2C B3 C8 4D 16 76 67 94 EA 11 DE 72 5C 09 AB 75 7C 9E D9 AD 58 
        53 29 B0 B4 DD 9D 32 97 54 34 E7 54 6E D9 82 9D 6F 4E 75 0D B4 59 D5 A3 28 A8 47 74 32 A1 D7 15 
        05 46 36 4E 0E 2C 0D C8 02 B3 B7 46 0C 3A 3C 3C E9 DB C7 BE F9 7A 7D 61 EF EE D9 56 98 BC 4A 38 
        C1 54 1E FB 29 F3 CF 03 27 B0 42 5F 53 56 D9 AF 54 A4 5C 52 BB D3 D8 BF 28 49 4C B8 0D C0 5A D3 
        AE 3C D7 43 9C D7 87 5E 76 C2 7C 4C 7C 39 E7 1B 33 64 7A 8D B3 A5 CF AF 36 BF 9D 38 80 61 80 A6 
        F6 67 78 D2 C8 EA DC 5C FF E9 C4 0F 79 CB 1F 89 06 A3 EA 4E 35 EA 97 A3 CE A9 63 F4 1F 6B DD A4 
        A7 AF C4 1C 2E C0 88 09 20 86 09 3C F6 56 E9 51 FA AC C2 D9 6C C7 13 5A D7 E8 79 5B F8 D4 1C 08 
        C2 00 6C 3A 9A 4C 87 31 48 B4 66 34 14 0B 05 25 D2 A1 28 1A 2F 23 0C 46 22 08 A9 50 CC 0D 18 8D 
        05 C3 81 C0 80 60 20 2A 11 01 A2 D1 80 B8 60 33 1B 48 8C 60 D9 38 C2 37 22 3B 83 4B 62 81 01 C0 
        5C 29 1B C2 84 05 28 38 C8 6C 28 16 C3 61 F1 18 98 D6 2D 22 8C 8C 45 C3 91 C0 D6 91 21 91 CB A5 
        80 D1 40 E0 62 29 2A 1A A8 D1 4A 4C 62 34 2E 8A 0C 86 F2 09 14 92 A5 33 14 4E 6B 72 51 94 9C 6C 
        34 10 0B 69 83 2B 3C D0 50 5C 19 0D 06 B0 71 B0 DC 62 28 2A 9C CC A2 02 61 04 52 34 18 0A 08 65 
        41 48 C6 60 28 C4 64 31 65 42 29 12 7A 49 27 11 C8 A5 22 E0 CC 64 35 29 88 0D E7 21 05 12 21 12 
        18 D2 22 F4 B1 98 B8 6B 2B A9 49 05 03 0A D5 73 55 15 D6 CB B6 1B 1B 96 CE AD 6C DC 51 EB F1 99 
        38 C8 62 38 BB 70 05 03 9D BD 77 75 4A DE 54 2E 36 6A 98 A0 4A 20 29 93 C8 84 92 A9 37 9F 74 D7 
        8E 06 D5 0B 9D D6 EF 8C 24 13 09 E5 22 49 10 8A 20 39 9B E0 47 43 49 BC DD C2 06 95 05 57 8C FA 
        DE 83 86 E1 90 64 14 0E 43 A8 DC 10 0C 23 A0 40 36 8C 23 48 DC 3A 0C A3 70 C2 37 0C 6C 28 E5 05 
        30 B0 38 D8 32 8E 63 9A 6F 05 0D 30 94 16 34 C3 F0 7B 50 B0 C0 6E 4B CE 17 2E CA 9A 6A 39 8D 03 
        78 C6 35 A0 E1 98 60 1B 85 0D 28 40 32 46 63 A4 76 D2 B1 F1 C8 C8 32 8E 50 F8 C2 10 0C C3 08 E7 
        08 B4 D0 C4 22 83 B5 6B 5C 50 89 2C AD DB 8E B9 05 A1 90 5C B9 2A 69 32 50 98 26 4B C0 9A 20 B3 
        4C B0 9C 20 89 C2 1B E2 29 32 61 88 72 14 32 CD 18 AA 29 B3 42 3C A8 23 07 2E 28 1B 2D B2 0F 32 
        E9 16 BD 2C 83 9F 2A CF 6A 62 C6 B2 B8 09 A8 40 22 27 CC 80 50 27 8A 02 98 5E 26 89 8D 3B 56 A0 
        8A 82 A8 85 3B 45 02 28 A8 06 A0 20 0D ";
}