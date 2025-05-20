using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LogicType
{
    VALUE,
    AND,
    OR,
    NOT
}

[Serializable]
public class ConditionGroup
{
    public string value;
    public LogicType logicType;
    public List<ConditionGroup> conditions;
}
