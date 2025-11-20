using System;
using System.Collections.Generic;

[Serializable]
public class RemoteConfigBundle
{
    public Dictionary<string, string> Strings = new();
    public Dictionary<string, double> Numbers = new();
    public Dictionary<string, bool> Bools = new();
    public Dictionary<string, object> Objects = new();
}