﻿using System;
#pragma warning disable IDE0079
#pragma warning disable CA1032

namespace Balu;

public abstract class BaluException : Exception
{
    private protected BaluException(string message) : base(message) { }
}
