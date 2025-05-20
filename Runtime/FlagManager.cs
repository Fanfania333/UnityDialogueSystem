using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum FlagType
{
    Bool, Int, Float, String
}

public class FlagManager : MonoBehaviour
{
    [Serializable]
    public struct StringIntPair
    {
        public string key;
        public int value;
    }
    
    [Serializable]
    public struct StringFloatPair
    {
        public string key;
        public float value;
    }
    
    [Serializable]
    public struct StringStringPair
    {
        public string key;
        public string value;
    }
    
    public static FlagManager Instance;
    
    private HashSet<string> boolFlags;
    [HideInInspector] public List<string> boolFlagSerialize;
    
    private Dictionary<string, int> intFlags;
    [HideInInspector] public List<StringIntPair> intFlagSerialize;
    
    private Dictionary<string, float> floatFlags;
    [HideInInspector] public List<StringFloatPair> floatFlagSerialize;
    
    private Dictionary<string, string> stringFlags;
    [HideInInspector] public List<StringStringPair> stringFlagSerialize;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        
        Instance = this;
    }

    private void Start()
    {
        boolFlags = new HashSet<string>();
        intFlags = new Dictionary<string,int>();
        floatFlags = new Dictionary<string, float>();
        stringFlags = new Dictionary<string, string>();
    }

    public bool AddFlag(string flag)
    {
        flag = flag.Trim();
        if (!FlagNameIsValid(flag)) return false;
        boolFlags.Add(flag);
        return true;
    }

    public void RemoveFlag(string flag)
    {
        flag = flag.Trim();
        boolFlags.Remove(flag);
    }

    public bool SetIntFlag(string flag, int value)
    {
        flag = flag.Trim();
        if (!FlagNameIsValid(flag)) return false;
        intFlags[flag] = value;
        return true;
    }

    public bool AddToIntFlag(string flag, int value)
    {
        flag = flag.Trim();
        if (!FlagNameIsValid(flag)) return false;
        if (!intFlags.TryAdd(flag, value))
        {
            intFlags[flag] = value + intFlags[flag];
        }
        return true;
    }

    public bool SetFloatFlag(string flag, float value)
    {
        flag = flag.Trim();
        if (!FlagNameIsValid(flag)) return false;
        floatFlags[flag] = value;
        return true;
    }
    
    public bool AddToFloatFlag(string flag, float value)
    {
        flag = flag.Trim();
        if (!FlagNameIsValid(flag)) return false;
        if (!floatFlags.TryAdd(flag, value))
        {
            floatFlags[flag] = value + floatFlags[flag];
        }
        return true;
    }
    
    public bool SetStringFlag(string flag, string value)
    {
        flag = flag.Trim();
        value = value.Trim();
        if (!FlagNameIsValid(flag)) return false;
        stringFlags[flag] = value;
        return true;
    }

    public bool HasFlag(string flag)
    {
        flag = flag.Trim();
        if (FlagNameIsValid(flag))
        {
            return boolFlags.Contains(flag);
        }

        string validStringRegex = "^[^><=!]+(<=|>=|!=|=|<|>)[^><=!]+$";
        if (!Regex.IsMatch(flag, validStringRegex))
        {
            Debug.LogWarning("Invalid flag format: " + flag);
            return false;
        }

        string operatorExtractRegex = "(<=|>=|!=|=|<|>)";
        string operatorString = Regex.Match(flag, operatorExtractRegex).Value;
        
        var pair = flag.Split(new string[]{operatorString}, StringSplitOptions.None);
        string key = pair[0];
        string value = pair[1];
        int intValue;
        float floatValue;

        bool isInt = int.TryParse(value, out intValue);
        bool isFloat = float.TryParse(value, out floatValue);

        if (isInt && intFlags.ContainsKey(key))
        {
            switch (operatorString)
            {
                case "=":
                    return intFlags[key] == intValue;
                case "!=":
                    return intFlags[key] != intValue;
                case ">":
                    return intFlags[key] > intValue;
                case "<":
                    return intFlags[key] < intValue;
                case ">=":
                    return intFlags[key] >= intValue;
                case "<=":
                    return intFlags[key] <= intValue;
            }
        }
        else if (isFloat && floatFlags.ContainsKey(key))
        {
            switch (operatorString)
            {
                case "=":
                    return floatFlags[key] == floatValue;
                case "!=":
                    return floatFlags[key] != floatValue;
                case ">":
                    return floatFlags[key] > floatValue;
                case "<":
                    return floatFlags[key] < floatValue;
                case ">=":
                    return floatFlags[key] >= floatValue;
                case "<=":
                    return floatFlags[key] <= floatValue;
            }
        }
        else if (!isInt && !isFloat && stringFlags.ContainsKey(key))
        {
            // Check for invalid operators.
            if (operatorString != "=" && operatorString != "!=")
            {
                Debug.LogWarning("Invalid operator " + operatorString + " for string flag.");
                return false;
            }

            if (operatorString == "=") return stringFlags[key] == value;
            
            return stringFlags[key] != value;
        }

        return false;
    }

    public bool EvaluateCondition(ConditionGroup group)
    {
        if (group == null)
        {
            return true;
        }
        
        if (group.logicType == LogicType.VALUE)
        {
            if (string.IsNullOrWhiteSpace(group.value))
            {
                return true;
            }
            
            return HasFlag(group.value);
        }

        if (group.conditions.Count == 0)
        {
            return true;
        }

        if (group.logicType == LogicType.OR)
        {
            foreach (var subGroup in group.conditions)
            {
                if (EvaluateCondition(subGroup))
                {
                    return true;
                }
            }
            
            return false;
        }

        if (group.logicType == LogicType.AND)
        {
            foreach (var subGroup in group.conditions)
            {
                if (!EvaluateCondition(subGroup))
                {
                    return false;
                }
            }
            
            return true;
        }

        if (group.logicType == LogicType.NOT)
        {
            if (group.conditions.Count > 1)
            {
                Debug.LogWarning("NOT logic type only supports one condition group, the rest will be ignored.");
            }
        
            return !EvaluateCondition(group.conditions[0]);
        }
        
        Debug.LogError("Invalid logic type: " + group.logicType);
        return false;
    }

    private bool FlagNameIsValid(string flagName)
    {
        return !Regex.IsMatch(flagName, @"[><=!]");
    }

    public void PrepareSerialization()
    {
        boolFlagSerialize = new List<string>(boolFlags);

        foreach (var pair in intFlags)
        {
            intFlagSerialize.Add(new StringIntPair{ key = pair.Key, value = pair.Value });
        }
        
        foreach (var pair in floatFlags)
        {
            floatFlagSerialize.Add(new StringFloatPair{ key = pair.Key, value = pair.Value });
        }
        
        foreach (var pair in stringFlags)
        {
            stringFlagSerialize.Add(new StringStringPair{ key = pair.Key, value = pair.Value });
        }
    }

    public void Deserialize(List<string> boolFlagList, List<StringIntPair> intFlagList, List<StringFloatPair> floatFlagList, List<StringStringPair> stringFlagList)
    {
        boolFlags = new HashSet<string>(boolFlagList);

        foreach (var pair in intFlagList)
        {
            intFlags.Add(pair.key, pair.value);
        }

        foreach (var pair in floatFlagList)
        {
            floatFlags.Add(pair.key, pair.value);
        }
        
        foreach (var pair in stringFlagList)
        {
            stringFlags.Add(pair.key, pair.value);
        }
    }
}
