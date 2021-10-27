using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct SimpleStruct
{
    public string stringValue;
}

[Serializable]
public class StructReferenceObject
{
    public string stringValue;
    public SimpleStruct simpleStruct;
}

[Serializable]
public class DateTimeObject
{
    public string stringValue;
    public DateTime created_at;
    public DateTime started_at;
}

[Serializable]
public class ReferenceObject3
{
    public string stringValue;
}

[Serializable]
public class ReferenceObject2
{
    public string stringValue;
    public ReferenceObject3 referenceObject3;
}

[Serializable]
public class ReferenceObject1
{
    public string stringValue;
    public ReferenceObject2 referenceObject2;
}

[Serializable]
public class RootObject
{
    public string stringValue;
    public ReferenceObject1 referenceObject1;
}

[Serializable]
public class Father
{
    public string fatherStringValue;
}

[Serializable]
public class Sun : Father
{
    public string stringValue;
}

[Serializable]
public class SunRootObject : RootObject
{
    public string sunStringValue;
}
