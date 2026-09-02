using System.Collections.Generic;

public enum FacialExpression
{
    Neutral,
    Happy,
    Sad,
    Angry,
    Surprised,
    Afraid,
    Disgusted,
    Contempt
}

[System.Serializable]
public struct DialogueLine
{
    public string Line;
    public float DisplayDuration;
    public FacialExpression Expression;
}

[System.Serializable]
public struct MorphTargetValue
{
    public string name;
    public float value;
    public float blendInTime;
}