using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    public int a;
    public ClassVar classVar;

    public Sample()
    {

    }
}

public class ClassVar
{
    public int b;
}

public class Test
{
    void Main()
    {
        var sample = new Sample()
        {
            a = 1,
            /*
             * http://var.blog.jp/archives/67628040.html
            classVar = new ClassVar()
            {
                b = 1
            },
            これを下のように書ける?
            */
            classVar = { b = 1 }
        };
    }
}
