﻿using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is the current stack of the operands for the postscript parser.
/// </summary>
public sealed class OperandStack : PostscriptStack<PostscriptValue>
{
    /// <summary>
    /// Create a new operand stack
    /// </summary>
    public OperandStack() : base(0,"")
    {
    }

    /// <inheritdoc />
    protected override void MakeCopyable(ref PostscriptValue value) => 
        value = value.AsCopyableValue();

    internal void PushCount() => Push(Count);

    private static bool IsMark(PostscriptValue i) => i.IsMark;

    /// <summary>
    /// Count the nu m be r of items above the first mark object
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PostscriptNamedErrorException"></exception>
    public int CountToMark()
    {
        var ret = CountAbove(IsMark);
        if (ret == Count)
            throw new PostscriptNamedErrorException("Could not find mark", "unmatchedmark");
        return ret;
    }

    internal void MarkedSpanToArray(bool asExecutable)
    {
        var array = PopTopToArray(CountToMark());
        Pop();
        var postscriptValue = PostscriptValueFactory.CreateArray(array);
        Push(asExecutable?postscriptValue.AsExecutable():postscriptValue);
    }

    internal void MarkedSpanToDictionary()
    {
        int count = CountToMark();
        var dict = PostscriptValueFactory.CreateDictionary(
            CollectionAsSpan()[^count..]);
        PopMultiple(count+1);
        Push(dict);
    }


    internal void CreatePackedArray() => 
        Push(
            PostscriptValueFactory.CreateArray(
                PopTopToArray(
                    Pop().Get<int>())));

    internal void ClearToMark() =>
        ClearThrough(IsMark);

    internal void PolymorphicCopy()
    {
        var topItem = Pop();
        if (topItem.TryGet(out IPostscriptComposite? dest))
            Push(dest.CopyFrom(Pop(), topItem));
        else
            CopyTop(topItem.Get<int>());
        
    }
}